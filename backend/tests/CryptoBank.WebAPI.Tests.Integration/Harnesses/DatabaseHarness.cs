using System.Collections;
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
    private bool _started;

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

        _started = true;
    }

    public async Task Stop()
    {
        _started = false;

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

    public async ValueTask<T> Execute<T>(Func<CryptoBankDbContext, ValueTask<T>> action)
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

    public Task Save<T>(T[] entities)
    {
        return Save(entities.Cast<object>().ToArray());
    }

    public async Task Save(params object[] entities)
    {
        ThrowIfNotStarted();

        await using var scope = _factory.Services.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();

        var collections = entities.OfType<IEnumerable>();

        foreach (var collection in collections)
        {
            dbContext.AddRange(collection);
        }

        var singleEntities = entities.Where(e => e is not IEnumerable);

        dbContext.AddRange(singleEntities);
        await dbContext.SaveChangesAsync();
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

    private void ThrowIfNotStarted()
    {
        if (_started)
            return;

        throw new InvalidOperationException($"DatabaseHarness is not started. Call {nameof(Start)} first.");
    }
}
