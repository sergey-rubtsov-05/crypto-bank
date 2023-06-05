using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank.Database;

public static class DatabaseProjectServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseProject(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CryptoBankDb"); //todo store password in secure place
        services.AddDbContext<CryptoBankDbContext>(optionsBuilder =>
            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseAdminDatabase("postgres")));

        return services;
    }
}
