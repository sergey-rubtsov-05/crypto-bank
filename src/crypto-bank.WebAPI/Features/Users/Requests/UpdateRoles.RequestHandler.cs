using crypto_bank.Infrastructure.Features.Users;
using JetBrains.Annotations;
using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class UpdateRoles
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request>
    {
        private readonly UserService _userService;

        public RequestHandler(UserService userService)
        {
            _userService = userService;
        }

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            await _userService.UpdateRoles(request.UserId, request.NewRoles);
        }
    }
}
