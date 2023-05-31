using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Database;

public static class DatabaseProjectServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseProject(this IServiceCollection services)
    {
        services.AddDbContext<CryptoBankDbContext>();

        return services;
    }
}
