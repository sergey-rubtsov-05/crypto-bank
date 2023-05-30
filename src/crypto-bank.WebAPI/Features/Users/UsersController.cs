using crypto_bank.WebAPI.Authorization;
using crypto_bank.WebAPI.Features.Users.Requests;
using crypto_bank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crypto_bank.WebAPI.Features.Users;

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
        return await _dispatcher.Dispatch(new GetProfile.Request(User), cancellationToken);
    }

    [Authorize(Policy = PolicyName.AdministratorRole)]
    [HttpPost("roles")]
    public async Task<IActionResult> UpdateRoles(
        UpdateRoles.Request request,
        CancellationToken cancellationToken)
    {
        await _dispatcher.Dispatch(request, cancellationToken);

        //todo: question: is it ok to use IActionResult here? Or should we use some kind of special response?
        return new NoContentResult();
    }
}
