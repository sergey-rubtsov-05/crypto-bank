using CryptoBank.WebAPI.Features.Users.Models;
using MediatR;

namespace CryptoBank.WebAPI.Features.Users.Requests;

public partial class GetProfile
{
    public record Request : IRequest<Response>;

    public record Response(UserModel Profile);
}
