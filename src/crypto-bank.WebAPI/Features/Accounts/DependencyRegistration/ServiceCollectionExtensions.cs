using crypto_bank.WebAPI.Features.Accounts.Options;
using crypto_bank.WebAPI.Features.Accounts.Requests;
using FluentValidation;

namespace crypto_bank.WebAPI.Features.Accounts.DependencyRegistration;

public static class AccountsServiceCollectionExtensions
{
    public static IServiceCollection AddAccounts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IValidator<Create.Request>, Create.RequestValidator>();
        services.Configure<AccountsOptions>(configuration.GetSection("Features:Accounts"));

        return services;
    }
}
