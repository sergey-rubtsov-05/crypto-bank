using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class UpdateRoles
{
    public record Request : IRequest<Response>;

    public record Response;
}
