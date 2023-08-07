using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CryptoBank.WebAPI.Tests.Integration.Harnesses;

public class DatabaseHarness<TProgram, TDbContext> : IHarness<TProgram>
    where TProgram : class
    where TDbContext : DbContext
{
    private string _connectionString;
    private WebApplicationFactory<TProgram> _factory;
    private PostgreSqlContainer _postgresContainer;

    public void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(
                    new Dictionary<string, string> { { "ConnectionStrings:CryptoBankDb", _connectionString } });
            });
    }

    public async Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken = default)
    {
        _factory = factory;

        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15.3-alpine")
            .Build();

        await _postgresContainer.StartAsync(cancellationToken);

        _connectionString = _postgresContainer.GetConnectionString();
    }

    public async Task Stop()
    {
        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();
    }

    public async Task Execute(Func<TDbContext, Task> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await action(dbContext);
    }

    public async Task<T> Execute<T>(Func<TDbContext, Task<T>> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await action(dbContext);
    }
}
