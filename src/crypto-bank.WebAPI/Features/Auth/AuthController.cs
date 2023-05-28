using crypto_bank.WebAPI.Features.Auth.Requests;
using crypto_bank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Mvc;

namespace crypto_bank.WebAPI.Features.Auth;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
    private readonly Dispatcher _dispatcher;

    public AuthController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<Authenticate.Response> Authenticate(
        Authenticate.Request request,
        CancellationToken cancellationToken)
    {
        return await _dispatcher.Dispatch(request, cancellationToken);
    }
}
