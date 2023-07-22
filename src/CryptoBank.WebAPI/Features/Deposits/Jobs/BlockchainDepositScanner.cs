using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Deposits.Options;
using CryptoBank.WebAPI.Features.Deposits.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace CryptoBank.WebAPI.Features.Deposits.Jobs;

public class BlockchainDepositScanner : BackgroundService
{
    private static int _lastUsedHeight = -1;

    private readonly BitcoinClientFactory _bitcoinClientFactory;
    private readonly IClock _clock;
    private readonly DepositsOptions _depositsOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BlockchainDepositScanner(
        BitcoinClientFactory bitcoinClientFactory,
        IClock clock,
        IOptions<DepositsOptions> depositsOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        _bitcoinClientFactory = bitcoinClientFactory;
        _clock = clock;
        _depositsOptions = depositsOptions.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            try
            {
                await InnerExecuteAsync(stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(_depositsOptions.BitcoinBlockchainScanInterval, stoppingToken);
        }
    }

    private async Task InnerExecuteAsync(CancellationToken stoppingToken)
    {
        var bitcoinClient = _bitcoinClientFactory.Create();

        //todo: potential problem: what if multiple instances of this service executing at the same time?
        var blockCount = await bitcoinClient.GetBlockCountAsync(stoppingToken);
        if (blockCount <= _lastUsedHeight)
            return;

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();
        //todo: in production we will have more than one deposit address
        var depositAddress = await dbContext.DepositAddresses.SingleOrDefaultAsync(stoppingToken);
        if (depositAddress == null)
            return;

        var cryptoAddress = depositAddress.CryptoAddress;

        for (var height = _lastUsedHeight + 1; height <= blockCount; height++)
        {
            var block = await bitcoinClient.GetBlockAsync(height, stoppingToken);
            var cryptoDeposits = block.Transactions
                .SelectMany(
                    transaction => transaction.Outputs,
                    (transaction, output) => new { Id = transaction.GetHash().ToString(), Output = output })
                .Select(
                    tx => new
                    {
                        tx.Id,
                        DestinationAddress =
                            tx.Output.ScriptPubKey.GetDestinationAddress(bitcoinClient.Network)?.ToString(),
                        Amount = tx.Output.Value,
                    })
                .Where(tx => tx.DestinationAddress == cryptoAddress)
                .Select(
                    tx => new CryptoDeposit(
                        depositAddress.UserId,
                        depositAddress.Id,
                        tx.Amount.ToDecimal(MoneyUnit.BTC),
                        "BTC",
                        _clock.UtcNow,
                        tx.Id))
                .ToList();

            foreach (var cryptoDeposit in cryptoDeposits)
            {
                dbContext.CryptoDeposits.Add(cryptoDeposit);
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }

        _lastUsedHeight = blockCount;
    }
}
