using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Auth.Requests;
using CryptoBank.WebAPI.Features.Auth.Services;
using FluentValidation;

namespace CryptoBank.WebAPI.Features.Auth.DependencyRegistration;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IValidator<Authenticate.Request>, Authenticate.RequestValidator>();
        services.AddScoped<IValidator<RefreshToken.Request>, RefreshToken.RequestValidator>();
        services.AddScoped<IValidator<RevokeToken.Request>, RevokeToken.RequestValidator>();

        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();
        services.Configure<AuthOptions>(configuration.GetSection("Features:Auth"));

        return services;
    }
}
