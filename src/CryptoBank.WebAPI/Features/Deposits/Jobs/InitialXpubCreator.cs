using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Deposits.Services;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.WebAPI.Features.Deposits.Jobs;

public class InitialXpubCreator : IHostedService
{
    private readonly ILogger<InitialXpubCreator> _logger;
    private readonly NetworkSource _networkSource;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public InitialXpubCreator(
        ILogger<InitialXpubCreator> logger,
        NetworkSource networkSource,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _networkSource = networkSource;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();

        //TODO: potential problem: if multiple instances of this service start at the same time
        var xpubExists = await dbContext.Xpubs.AnyAsync(cancellationToken);
        if (xpubExists)
            return;

        var masterPubKey = GeneratePubKey();

        await SaveToDb(masterPubKey, dbContext, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private string GeneratePubKey()
    {
        var masterPrvKey = new ExtKey();
        var network = _networkSource.Get();
        _logger.LogInformation($"Master private key: [{masterPrvKey.ToString(network)}]");

        var masterPubKey = masterPrvKey.Neuter();
        var masterPubKeyAsString = masterPubKey.ToString(network);
        _logger.LogInformation($"Master public key: [{masterPubKeyAsString}]");
        return masterPubKeyAsString;
    }

    private static async Task SaveToDb(
        string masterPubKey,
        CryptoBankDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var xpubEntity = new Xpub("BTC", masterPubKey);
        await dbContext.Xpubs.AddAsync(xpubEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
