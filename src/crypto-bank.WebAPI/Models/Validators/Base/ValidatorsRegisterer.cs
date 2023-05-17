using FluentValidation;

namespace crypto_bank.WebAPI.Models.Validators.Base;

public static class ValidatorsRegisterer
{
    public static IServiceCollection AddApiModelValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<UserRegistrationRequest>, UserRegistrationRequestValidator>();

        return services;
    }
}
