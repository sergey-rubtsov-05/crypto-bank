using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Common;

public static class CommonProjectServiceCollectionExtensions
{
    public static IServiceCollection AddCommonProject(this IServiceCollection services)
    {
        services.AddSingleton<IClock, Clock>();

        return services;
    }
}
