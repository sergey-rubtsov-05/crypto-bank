using crypto_bank.WebAPI.Common.Services;

namespace crypto_bank.WebAPI.Common;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.Configure<Argon2ConfigOptions>(configuration.GetSection("Argon2ConfigOptions"));

        return services;
    }
}
