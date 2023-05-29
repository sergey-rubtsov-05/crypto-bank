using crypto_bank.WebAPI.Features.Users.Requests;
using crypto_bank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crypto_bank.WebAPI.Features.Users;

[ApiController]
[Route("/users")]
public class UserController : Controller
{
    private readonly Dispatcher _dispatcher;

    public UserController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<Register.Response> Register(Register.Request request, CancellationToken cancellationToken)
    {
        return await _dispatcher.Dispatch(request, cancellationToken);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<GetProfile.Response> GetProfile(CancellationToken cancellationToken)
    {
        return await _dispatcher.Dispatch(new GetProfile.Request(User), cancellationToken);
    }
}
