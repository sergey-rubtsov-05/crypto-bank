using crypto_bank.WebAPI.Features.Accounts.Models;
using MediatR;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public partial class GetList
{
    public record Request : IRequest<Response>;

    public record Response(IReadOnlyList<AccountModel> Accounts);
}
