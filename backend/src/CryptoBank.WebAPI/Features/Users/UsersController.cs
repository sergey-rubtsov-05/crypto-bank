using CryptoBank.WebAPI.Authorization;
using CryptoBank.WebAPI.Features.Users.Requests;
using CryptoBank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebAPI.Features.Users;

[ApiController]
[Route("/users")]
public class UsersController : Controller
{
    private readonly Dispatcher _dispatcher;

    public UsersController(Dispatcher dispatcher)
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
        return await _dispatcher.Dispatch(new GetProfile.Request(), cancellationToken);
    }

    [Authorize(Policy = PolicyName.AdministratorRole)]
    [HttpPost("roles")]
    public async Task<IActionResult> UpdateRoles(
        UpdateRoles.Request request,
        CancellationToken cancellationToken)
    {
        await _dispatcher.Dispatch(request, cancellationToken);

        return new NoContentResult();
    }
}
