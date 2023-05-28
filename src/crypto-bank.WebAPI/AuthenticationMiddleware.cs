using crypto_bank.Infrastructure.Features.Auth;
using crypto_bank.Infrastructure.Features.Auth.Exceptions;
using Microsoft.AspNetCore.Http.Features;

namespace crypto_bank.WebAPI;

public class AuthenticationMiddleware : IMiddleware
{
    private readonly TokenService _tokenService;

    public AuthenticationMiddleware(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await Validate(context);

        await next(context);
    }

    private async Task Validate(HttpContext context)
    {
        var endpointFeature = context.Features.Get<IEndpointFeature>();
        var authenticationAttribute = endpointFeature?.Endpoint?.Metadata.GetMetadata<AuthenticationAttribute>();
        if (authenticationAttribute is null)
            return;

        var authorizationHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
            throw new AuthenticationException("Authorization header is missing or empty");

        var token = authorizationHeader.Split(' ')[1];

        await _tokenService.Validate(token);
    }
}
