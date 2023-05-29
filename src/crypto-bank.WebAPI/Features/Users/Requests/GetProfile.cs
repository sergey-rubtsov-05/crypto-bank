using System.Security.Claims;
using crypto_bank.WebAPI.Features.Users.Models;
using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class GetProfile
{
    public record Request(ClaimsPrincipal Principal) : IRequest<Response>;

    public record Response(UserModel Profile);
}
