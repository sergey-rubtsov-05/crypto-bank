using crypto_bank.WebAPI.Features.Users.Options;
using crypto_bank.WebAPI.Features.Users.Requests;
using FluentValidation;

namespace crypto_bank.WebAPI.Features.Users.DependencyRegistration;

public static class UsersServiceCollectionExtensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IValidator<Register.Request>, Register.RequestValidator>();
        services.AddScoped<IValidator<UpdateRoles.Request>, UpdateRoles.RequestValidator>();

        services.Configure<UsersOptions>(configuration.GetSection("Features:Users"));

        return services;
    }
}
