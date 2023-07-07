using CryptoBank.WebAPI.Features.Deposits.Jobs;

namespace CryptoBank.WebAPI.Features.Deposits.DependencyRegistration;

public static class DepositsServiceCollectionExtensions
{
    public static IServiceCollection AddDeposits(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<InitialXpubCreator>();

        return services;
    }
}
