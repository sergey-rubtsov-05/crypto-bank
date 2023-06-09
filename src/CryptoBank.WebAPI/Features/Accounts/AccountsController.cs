using CryptoBank.WebAPI.Authorization;
using CryptoBank.WebAPI.Features.Accounts.Requests;
using CryptoBank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebAPI.Features.Accounts;

[ApiController]
[Route("/accounts")]
public class AccountsController : Controller
{
    private readonly Dispatcher _dispatcher;

    public AccountsController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<Create.Response> Create(Create.Request request, CancellationToken cancellationToken)
    {
        return await _dispatcher.Dispatch(request, cancellationToken);
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<GetList.Response> GetList(CancellationToken cancellationToken)
    {
        return await _dispatcher.Dispatch(new GetList.Request(), cancellationToken);
    }

    [Authorize(Policy = PolicyName.AnalystRole)]
    [HttpGet("numberOfOpenedAccounts")]
    public async Task<GetNumberOfOpenedAccounts.Response> GetNumberOfOpenedAccounts(
        [FromQuery] GetNumberOfOpenedAccounts.Request request,
        CancellationToken cancellationToken)
    {
        return await _dispatcher.Dispatch(request, cancellationToken);
    }
}
