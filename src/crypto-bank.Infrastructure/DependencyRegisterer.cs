using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Infrastructure;

public static class DependencyRegisterer
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<UserService>();

        return services;
    }
}
