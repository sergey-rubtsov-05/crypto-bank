using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace crypto_bank.Database;

public class DatabaseMigrator : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DatabaseMigrator(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
        var cryptoBankDb = serviceScope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();
        await cryptoBankDb.Database.EnsureCreatedAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //todo: use migration instead of drop database
        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
        var cryptoBankDb = serviceScope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();
        await cryptoBankDb.Database.EnsureDeletedAsync(cancellationToken);
    }
}
