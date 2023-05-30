using MediatR;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public partial class GetNumberOfOpenedAccounts
{
    public record Request(DateTimeOffset Begin, DateTimeOffset End) : IRequest<Response>;

    public record Response(int NumberOfAccounts);
}
