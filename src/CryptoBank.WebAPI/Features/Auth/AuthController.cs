using CryptoBank.WebAPI.Authorization;
using CryptoBank.WebAPI.Features.Auth.Requests;
using CryptoBank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebAPI.Features.Auth;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
    private const string RefreshTokenCookieKey = "refresh-token";
    private readonly Dispatcher _dispatcher;

    public AuthController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<Authenticate.Response> Authenticate(
        Authenticate.Request request,
        CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(request, cancellationToken);

        SetRefreshTokenToCookies(response.RefreshToken);

        return response;
    }

    [AllowAnonymous]
    [HttpPost("refreshToken")]
    public async Task<RefreshToken.Response> RefreshToken(CancellationToken cancellationToken)
    {
        //todo: question: is there better way to load value from cookies to request model
        var request = new RefreshToken.Request(GetRefreshTokenFromCookies());

        var response = await _dispatcher.Dispatch(request, cancellationToken);

        SetRefreshTokenToCookies(response.RefreshToken);

        return response;
    }

    [Authorize(Policy = PolicyName.AdministratorRole)]
    [HttpPost("revokeRefreshToken")]
    public async Task<IActionResult> RevokeToken(RevokeToken.Request request, CancellationToken cancellationToken)
    {
        await _dispatcher.Dispatch(request, cancellationToken);

        return NoContent();
    }

    private string? GetRefreshTokenFromCookies()
    {
        var refreshToken = HttpContext.Request.Cookies[RefreshTokenCookieKey];
        return refreshToken;
    }

    private void SetRefreshTokenToCookies(string refreshToken)
    {
        //todo: question is there better to set cookie in response
        HttpContext.Response.Cookies.Append(
            RefreshTokenCookieKey,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict,
                // todo: question: do I need to set expires for cookie, because token has it?
            });
    }
}
