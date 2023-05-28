using crypto_bank.Infrastructure.Features.Users.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Infrastructure.Features.Users;

public static class UsersServiceCollectionExtensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<UserService>();
        services.Configure<UsersOptions>(configuration.GetSection("Features:Users"));

        return services;
    }
}
