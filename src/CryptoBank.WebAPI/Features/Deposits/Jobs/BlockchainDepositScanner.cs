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
        var addresses = await dbContext.DepositAddresses.ToListAsync(stoppingToken);

        if (!addresses.Any())
            return;

        for (var height = _lastUsedHeight + 1; height <= blockCount; height++)
        {
            var block = await bitcoinClient.GetBlockAsync(height, stoppingToken);

            var deposits = GetDeposits(block, bitcoinClient.Network, addresses).ToList();

            if (deposits.Any())
            {
                dbContext.CryptoDeposits.AddRange(deposits);
                await dbContext.SaveChangesAsync(stoppingToken);
            }

            _lastUsedHeight = height;
        }
    }

    private IEnumerable<CryptoDeposit> GetDeposits(Block block, Network network, IEnumerable<DepositAddress> addresses)
    {
        var transactions = block.Transactions
            .SelectMany(
                transaction => transaction.Outputs,
                (transaction, output) => new
                {
                    Id = transaction.GetHash().ToString(),
                    DestinationAddress =
                        output.ScriptPubKey.GetDestinationAddress(network)?.ToString(),
                    Amount = output.Value,
                });

        var deposits = transactions
            .Join(
                addresses,
                transaction => transaction.DestinationAddress,
                address => address.CryptoAddress,
                (transaction, address) => new CryptoDeposit(
                    address.UserId,
                    address.Id,
                    transaction.Amount.ToDecimal(MoneyUnit.BTC),
                    address.CurrencyCode,
                    _clock.UtcNow,
                    transaction.Id));

        return deposits;
    }

    public override void Dispose()
    {
        _lastUsedHeight = -1;
        base.Dispose();
    }
}
