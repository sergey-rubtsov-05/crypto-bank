using CryptoBank.WebAPI.Features.Auth.Requests;
using CryptoBank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebAPI.Features.Auth;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
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
        return await _dispatcher.Dispatch(request, cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("refreshToken")]
    public async Task<RefreshToken.Response> RefreshToken(
        RefreshToken.Request request,
        CancellationToken cancellationToken)
    {
        return await _dispatcher.Dispatch(request, cancellationToken);
    }
}
