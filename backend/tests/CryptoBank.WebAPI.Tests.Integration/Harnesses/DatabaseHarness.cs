using System.Data;
using System.Linq.Expressions;
using CryptoBank.Database;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace CryptoBank.WebAPI.Tests.Integration.Harnesses;

public class DatabaseHarness<TProgram> : IHarness<TProgram> where TProgram : class
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

    public async Task Execute(Func<CryptoBankDbContext, Task> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();
        await action(dbContext);
    }

    public async Task<T> Execute<T>(Func<CryptoBankDbContext, Task<T>> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();
        return await action(dbContext);
    }

    public async Task<T> SingleOrDefaultAsync<T>(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken)
        where T : class
    {
        var actualDeposit = await Execute(
            dbContext => dbContext.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken));

        return actualDeposit;
    }

    public async Task Clear(CancellationToken cancellationToken)
    {
        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();

        await using var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                SchemasToInclude = new[] { "public" },
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = new[] { new Table("xpubs") },
            });

        await respawner.ResetAsync(connection);
    }
}