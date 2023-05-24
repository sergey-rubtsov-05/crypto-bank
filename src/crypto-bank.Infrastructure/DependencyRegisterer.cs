using crypto_bank.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Infrastructure;

public static class DependencyRegisterer
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddScoped<UserService>();

        services.AddScoped<TokenService>();
        services.Configure<TokenOptions>(configuration.GetSection("TokenOptions"));

        return services;
    }
}
