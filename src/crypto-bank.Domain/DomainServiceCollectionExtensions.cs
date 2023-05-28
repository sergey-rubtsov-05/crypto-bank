using crypto_bank.Domain.Features.Users.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace crypto_bank.Domain;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IValidator<User>, UserValidator>();

        return services;
    }
}
