using Microsoft.OpenApi.Models;

namespace CryptoBank.WebAPI.Swagger;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(
            options =>
            {
                options.CustomSchemaIds(type => type.ToString().Replace("+", "_"));
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        BearerFormat = "jwt",
                        Scheme = "Bearer",
                    });

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme },
                            },
                            Array.Empty<string>()
                        },
                    });
            });

        return services;
    }
}
