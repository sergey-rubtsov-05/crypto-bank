using CryptoBank.WebAPI.Features.Deposits.InitializationTasks;
using CryptoBank.WebAPI.Features.Deposits.Jobs;
using CryptoBank.WebAPI.Features.Deposits.Options;
using CryptoBank.WebAPI.Features.Deposits.Services;

namespace CryptoBank.WebAPI.Features.Deposits.DependencyRegistration;

public static class DepositsServiceCollectionExtensions
{
    public static IServiceCollection AddDeposits(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<InitialXpubCreator>();
        services.AddHostedService<BlockchainDepositScanner>();
        services.Configure<DepositsOptions>(configuration.GetSection("Features:Deposits"));
        services.AddSingleton<NetworkSource>();
        services.AddSingleton<BitcoinClientFactory>();

        return services;
    }
}
