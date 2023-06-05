using CryptoBank.WebAPI.Common.Services;

namespace CryptoBank.WebAPI.Common;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.Configure<Argon2ConfigOptions>(configuration.GetSection("Argon2ConfigOptions"));

        services.AddScoped<CurrentAuthInfoSource>();

        return services;
    }
}
