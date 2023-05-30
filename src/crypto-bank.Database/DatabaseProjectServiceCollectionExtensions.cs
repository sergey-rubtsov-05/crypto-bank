using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Database;

public static class DatabaseProjectServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseProject(this IServiceCollection services)
    {
        //todo move password to secrets
        var connectionString = "Host=localhost;Port=5432;Database=crypto_bank_db;Username=postgres;Password=nonsecret";
        services.AddDbContext<CryptoBankDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseAdminDatabase("postgres")));
        services.AddHostedService<DatabaseMigrator>();
        return services;
    }
}
