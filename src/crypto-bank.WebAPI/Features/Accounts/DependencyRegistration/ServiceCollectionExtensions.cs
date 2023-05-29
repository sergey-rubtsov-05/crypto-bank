using crypto_bank.WebAPI.Features.Accounts.Requests;
using FluentValidation;

namespace crypto_bank.WebAPI.Features.Accounts.DependencyRegistration;

public static class AccountsServiceCollectionExtensions
{
    public static IServiceCollection AddAccounts(this IServiceCollection services)
    {
        services.AddScoped<IValidator<Create.Request>, Create.RequestValidator>();

        return services;
    }
}
