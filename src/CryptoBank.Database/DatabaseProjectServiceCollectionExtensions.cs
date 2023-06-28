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
        //todo store password in secure place
        services.AddDbContext<CryptoBankDbContext>(
            optionsBuilder => optionsBuilder.UseNpgsql(
                configuration.GetConnectionString("CryptoBankDb"),
                npgsqlOptions => npgsqlOptions.UseAdminDatabase("postgres")));

        return services;
    }
}
