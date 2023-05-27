using crypto_bank.Domain.Validation;
using crypto_bank.Infrastructure;

namespace crypto_bank.WebAPI;

public static class DependencyContainer
{
    public static void RegisterDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddDomainModelValidators();
        builder.Services.AddScoped<ExceptionHandlerMiddleware>();
        builder.Services.AddScoped<AuthenticationMiddleware>();
    }
}
