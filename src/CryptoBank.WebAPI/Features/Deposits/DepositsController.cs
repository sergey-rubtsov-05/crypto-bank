using CryptoBank.WebAPI.Features.Deposits.Requests;
using CryptoBank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebAPI.Features.Deposits;

[ApiController]
[Route("/deposits")]
public class DepositsController
{
    private readonly Dispatcher _dispatcher;

    public DepositsController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [Authorize]
    [HttpPut("depositAddress")]
    public Task<GetDepositAddress.Response> GetDepositAddress(
        GetDepositAddress.Request request,
        CancellationToken cancellationToken)
    {
        return _dispatcher.Dispatch(request, cancellationToken);
    }
}
