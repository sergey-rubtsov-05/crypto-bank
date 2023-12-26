using CryptoBank.WebAPI.Features.Accounts.Options;
using CryptoBank.WebAPI.Features.Accounts.Requests;
using FluentValidation;

namespace CryptoBank.WebAPI.Features.Accounts.DependencyRegistration;

public static class AccountsServiceCollectionExtensions
{
    public static IServiceCollection AddAccounts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IValidator<Create.Request>, Create.RequestValidator>();
        services.AddScoped<IValidator<GetNumberOfOpenedAccounts.Request>, GetNumberOfOpenedAccounts.RequestValidator>();
        services.Configure<AccountsOptions>(configuration.GetSection("Features:Accounts"));

        return services;
    }
}
