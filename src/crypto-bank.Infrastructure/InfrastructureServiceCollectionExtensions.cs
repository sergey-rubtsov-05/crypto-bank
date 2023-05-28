using crypto_bank.Infrastructure.Authentication;
using crypto_bank.Infrastructure.Common;
using crypto_bank.Infrastructure.Features.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddUsers();

        services.AddScoped<TokenService>();
        services.Configure<TokenOptions>(configuration.GetSection("TokenOptions"));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
