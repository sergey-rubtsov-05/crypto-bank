using CryptoBank.WebAPI.Features.Accounts.Models;
using MediatR;

namespace CryptoBank.WebAPI.Features.Accounts.Requests;

public partial class GetList
{
    public record Request : IRequest<Response>;

    public record Response(IReadOnlyList<AccountModel> Accounts);
}
