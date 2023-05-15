using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<CryptoBankDb>(options => options.UseInMemoryDatabase("CryptoBankDb"));
        return services;
    }
}
