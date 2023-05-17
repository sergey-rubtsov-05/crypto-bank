using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Domain.Validators.Base;

public static class ValidatorsRegisterer
{
    public static IServiceCollection AddDomainModelValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<User>, UserValidator>();

        return services;
    }
}
