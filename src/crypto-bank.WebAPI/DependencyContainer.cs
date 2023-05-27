using crypto_bank.Domain.Models.Validators.Base;
using crypto_bank.Infrastructure;

namespace crypto_bank.WebAPI;

public static class DependencyContainer
{
    public static void RegisterDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddDomainModelValidators();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddScoped<ExceptionHandlerMiddleware>();
        builder.Services.AddScoped<AuthenticationMiddleware>();
    }
}
