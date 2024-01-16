using CryptoBank.Database;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CryptoBank.WebAPI.Tests.Integration.Common;

[Obsolete("Use TestFixture with Harnesses instead.")]
public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected CryptoBankDbContext DbContext;
    protected AsyncServiceScope Scope;

    protected IntegrationTestsBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder => builder
                    .ConfigureServices(ConfigureService)
                    .ConfigureAppConfiguration(
                        (_, configBuilder) =>
                        {
                            configBuilder.AddInMemoryCollection(
                                new Dictionary<string, string>
                                {
                                    {
                                        "ConnectionStrings:CryptoBankDb",
                                        "Host=localhost;Database=crypto_bank_db.tests;Username=integration_tests;Password=12345678;Maximum Pool Size=10;Connection Idle Lifetime=60;"
                                    },
                                });
                        }));
    }

    public virtual Task InitializeAsync()
    {
        var _ = Factory.Server;
        Scope = Factory.Services.CreateAsyncScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();

        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await Scope.DisposeAsync();
    }

    protected virtual void ConfigureService(IServiceCollection services)
    {
    }
}
