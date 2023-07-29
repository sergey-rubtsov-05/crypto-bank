using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Deposits.Options;
using CryptoBank.WebAPI.Features.Deposits.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Features.Deposits.Jobs;

public class BlockchainDepositScanner : BackgroundService
{
    private readonly BitcoinClientFactory _bitcoinClientFactory;
    private readonly IClock _clock;
    private readonly DepositsOptions _depositsOptions;
    private readonly ILogger<BlockchainDepositScanner> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BlockchainDepositScanner(
        BitcoinClientFactory bitcoinClientFactory,
        IClock clock,
        IOptions<DepositsOptions> depositsOptions,
        ILogger<BlockchainDepositScanner> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _bitcoinClientFactory = bitcoinClientFactory;
        _clock = clock;
        _depositsOptions = depositsOptions.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await InnerExecuteAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while scanning bitcoin blockchain");
            }

            await Task.Delay(_depositsOptions.BitcoinBlockchainScanInterval, cancellationToken);
        }
    }

    private async Task InnerExecuteAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();
        var bitcoinClient = _bitcoinClientFactory.Create();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var lastProcessedBlockHeight = await GetLastProcessedBlockHeight(dbContext, bitcoinClient, cancellationToken);

        var blockCount = await bitcoinClient.GetBlockCountAsync(cancellationToken);
        if (blockCount <= lastProcessedBlockHeight)
            return;

        var addresses = await dbContext.DepositAddresses.ToListAsync(cancellationToken);

        if (!addresses.Any())
            return;

        for (var height = lastProcessedBlockHeight + 1; height <= blockCount; height++)
        {
            var block = await bitcoinClient.GetBlockAsync(height, cancellationToken);

            var deposits = GetDeposits(block, bitcoinClient.Network, addresses).ToList();

            if (deposits.Any())
            {
                dbContext.CryptoDeposits.AddRange(deposits);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await dbContext.BitcoinBlockchainStatuses.ExecuteUpdateAsync(
                calls => calls.SetProperty(status => status.LastProcessedBlockHeight, height),
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task<int> GetLastProcessedBlockHeight(
        CryptoBankDbContext dbContext,
        RPCClient bitcoinClient,
        CancellationToken cancellationToken)
    {
        var bitcoinBlockchainStatus = await dbContext.BitcoinBlockchainStatuses
            .FromSql($"SELECT * FROM bitcoin_blockchain_statuses FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (bitcoinBlockchainStatus != null)
            return bitcoinBlockchainStatus.LastProcessedBlockHeight;

        var currentHeight = await bitcoinClient.GetBlockCountAsync(cancellationToken);

        var lastProcessedBlockHeight = bitcoinBlockchainStatus?.LastProcessedBlockHeight ?? currentHeight - 1;
        dbContext.BitcoinBlockchainStatuses.Add(new BitcoinBlockchainStatus(lastProcessedBlockHeight));
        await dbContext.SaveChangesAsync(cancellationToken);
        return lastProcessedBlockHeight;
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
}
