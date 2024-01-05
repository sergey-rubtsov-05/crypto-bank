using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank.Common;

public static class CommonProjectServiceCollectionExtensions
{
    public static IServiceCollection AddCommonProject(this IServiceCollection services)
    {
        services.AddSingleton<IClock, Clock>();

        return services;
    }
}
