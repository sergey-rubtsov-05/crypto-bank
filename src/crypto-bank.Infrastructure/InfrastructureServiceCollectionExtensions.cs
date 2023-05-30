using crypto_bank.Infrastructure.Common;
using crypto_bank.Infrastructure.Features.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddAuth(configuration);

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
