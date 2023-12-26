using FluentValidation;

namespace CryptoBank.WebAPI.Common.Validation;

public static class ValidationServiceCollectionExtensions
{
    public static IServiceCollection EnsureValidatorsAreRegistered<T>(this IServiceCollection services)
    {
        AssemblyScanner.FindValidatorsInAssemblyContaining<T>().ForEach(scanResult =>
        {
            var isValidatorRegistered = services.Any(descriptor => descriptor.ServiceType == scanResult.InterfaceType);
            if (isValidatorRegistered)
                return;

            throw new ValidatorRegistrationException($"Validator for {scanResult.InterfaceType} is not registered");
        });

        return services;
    }
}
