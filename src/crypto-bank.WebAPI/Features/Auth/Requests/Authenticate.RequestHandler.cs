using crypto_bank.Infrastructure.Features.Auth;
using JetBrains.Annotations;
using MediatR;

namespace crypto_bank.WebAPI.Features.Auth.Requests;

public partial class Authenticate
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AuthService _authService;

        public RequestHandler(AuthService authService)
        {
            _authService = authService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var accessToken = await _authService.Authenticate(request.Email, request.Password);

            return new Response(accessToken);
        }
    }
}
