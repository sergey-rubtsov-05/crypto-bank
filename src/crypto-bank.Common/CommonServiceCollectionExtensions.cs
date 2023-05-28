using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Common;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        services.AddSingleton<IClock, Clock>();

        return services;
    }
}
