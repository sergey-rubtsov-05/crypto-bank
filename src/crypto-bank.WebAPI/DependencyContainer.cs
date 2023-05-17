using crypto_bank.Database;
using crypto_bank.Domain.Models.Validators.Base;
using crypto_bank.Infrastructure;
using crypto_bank.WebAPI.Models.Validators.Base;

namespace crypto_bank.WebAPI;

public static class DependencyContainer
{
    public static void RegisterDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
        builder.Services.AddDatabase();
        builder.Services.AddDomainModelValidators();
        builder.Services.AddApiModelValidators();
        builder.Services.AddInfrastructure();
        builder.Services.AddScoped<ExceptionHandlerMiddleware>();
    }
}
