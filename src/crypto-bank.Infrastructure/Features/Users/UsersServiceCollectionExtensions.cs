using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Infrastructure.Features.Users;

public static class UsersServiceCollectionExtensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services)
    {
        services.AddScoped<UserService>();

        return services;
    }
}
