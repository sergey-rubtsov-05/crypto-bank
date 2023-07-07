using CryptoBank.Database;
using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.WebAPI.Features.Deposits.Jobs;

public class InitialXpubCreator : IHostedService
{
    private readonly ILogger<InitialXpubCreator> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public InitialXpubCreator(IServiceScopeFactory serviceScopeFactory, ILogger<InitialXpubCreator> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();

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
        //TODO move Network.TestNet to config
        var network = Network.TestNet;
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
