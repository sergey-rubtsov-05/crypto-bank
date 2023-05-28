using crypto_bank.Infrastructure.Features.Auth.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Infrastructure.Features.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<TokenService>();
        services.Configure<AuthOptions>(configuration.GetSection("Features:Auth"));

        return services;
    }
}
