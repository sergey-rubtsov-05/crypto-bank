using crypto_bank.WebAPI.Common.Services;

namespace crypto_bank.WebAPI.Common;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
