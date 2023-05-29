using crypto_bank.Infrastructure.Features.Users;
using JetBrains.Annotations;
using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class UpdateRoles
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
            await _userService.UpdateRoles(request.UserId, request.NewRoles);

            return new Response(); //todo: why I return empty response? Could I return just HttpStatusCode.OK?
        }
    }
}
