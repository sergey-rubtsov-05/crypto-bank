using crypto_bank.WebAPI.Features.Auth.Options;
using crypto_bank.WebAPI.Features.Auth.Requests;
using crypto_bank.WebAPI.Features.Auth.Services;
using FluentValidation;

namespace crypto_bank.WebAPI.Features.Auth.DependencyRegistration;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IValidator<Authenticate.Request>, Authenticate.RequestValidator>();

        services.AddScoped<TokenService>();
        services.Configure<AuthOptions>(configuration.GetSection("Features:Auth"));

        return services;
    }
}
