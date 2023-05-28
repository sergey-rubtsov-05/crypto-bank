using crypto_bank.WebAPI.Features.Auth.Requests;
using FluentValidation;

namespace crypto_bank.WebAPI.Features.Auth.DependencyRegistration;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<IValidator<Authenticate.Request>, Authenticate.RequestValidator>();

        return services;
    }
}
