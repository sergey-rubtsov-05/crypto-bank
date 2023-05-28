using crypto_bank.Infrastructure.Features.Users;
using JetBrains.Annotations;
using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class Register
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly UserService _userService;

        public RequestHandler(UserService userService)
        {
            _userService = userService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userService.Register(request.Email, request.Password, request.BirthDate);

            return new Response(user.Id);
        }
    }
}
