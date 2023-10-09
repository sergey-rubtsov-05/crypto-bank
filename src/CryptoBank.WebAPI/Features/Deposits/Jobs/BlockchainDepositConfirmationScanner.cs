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

public class BlockchainDepositConfirmationScanner : BackgroundService
{
    private readonly BitcoinClientFactory _bitcoinClientFactory;
    private readonly IClock _clock;
    private readonly DepositsOptions _depositsOptions;
    private readonly ILogger<BlockchainDepositConfirmationScanner> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BlockchainDepositConfirmationScanner(
        BitcoinClientFactory bitcoinClientFactory,
        IClock clock,
        IOptions<DepositsOptions> depositsOptions,
        ILogger<BlockchainDepositConfirmationScanner> logger,
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
        while (!cancellationToken.IsCancellationRequested)
        {
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

        while (true)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var deposit = await LoadUnconfirmedDeposit(dbContext, cancellationToken);

            if (deposit is null)
                return;

            var confirmations = await LoadConfirmationsCount(bitcoinClient, deposit.TxId, cancellationToken);

            if (confirmations < _depositsOptions.BitcoinTxConfirmationCount)
                await UpdateDepositAsPending(dbContext, deposit.Id, confirmations, cancellationToken);
            else
                await UpdateDepositAsConfirmed(dbContext, deposit.Id, confirmations, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }

    private async Task<CryptoDeposit?> LoadUnconfirmedDeposit(
        CryptoBankDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.CryptoDeposits
            .FromSql(
                @$"
SELECT *
FROM crypto_deposits
WHERE status != {DepositStatus.Confirmed} AND
      ((scanned_at IS NULL) OR
       (scanned_at + {_depositsOptions.BitcoinBlockchainScanInterval} < {_clock.UtcNow}))
ORDER BY scanned_at
FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task<uint> LoadConfirmationsCount(
        RPCClient bitcoinClient,
        string depositTxId,
        CancellationToken cancellationToken)
    {
        const bool asJson = true;
        var rpcResponse = await bitcoinClient.SendCommandAsync(
            RPCOperations.getrawtransaction,
            cancellationToken,
            uint256.Parse(depositTxId),
            asJson);

        var confirmations = rpcResponse.Result.Value<uint?>("confirmations") ?? 0;
        return confirmations;
    }

    private async Task UpdateDepositAsPending(
        CryptoBankDbContext dbContext,
        long depositId,
        uint confirmationsCount,
        CancellationToken cancellationToken)
    {
        await dbContext.CryptoDeposits
            .Where(x => x.Id == depositId)
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(x => x.Confirmations, confirmationsCount)
                    .SetProperty(x => x.Status, DepositStatus.Pending)
                    .SetProperty(x => x.ScannedAt, _clock.UtcNow),
                cancellationToken);
    }

    private async Task UpdateDepositAsConfirmed(
        CryptoBankDbContext dbContext,
        long depositId,
        uint confirmationsCount,
        CancellationToken cancellationToken)
    {
        await dbContext.CryptoDeposits
            .Where(x => x.Id == depositId)
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(x => x.Confirmations, confirmationsCount)
                    .SetProperty(x => x.Status, DepositStatus.Confirmed)
                    .SetProperty(x => x.ConfirmedAt, _clock.UtcNow),
                cancellationToken);
    }
}
