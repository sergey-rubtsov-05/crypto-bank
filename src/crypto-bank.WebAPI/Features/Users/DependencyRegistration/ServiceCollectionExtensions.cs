using crypto_bank.WebAPI.Features.Users.Requests;
using FluentValidation;

namespace crypto_bank.WebAPI.Features.Users.DependencyRegistration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services)
    {
        services.AddScoped<IValidator<Register.Request>, Register.RequestValidator>();

        return services;
    }
}
