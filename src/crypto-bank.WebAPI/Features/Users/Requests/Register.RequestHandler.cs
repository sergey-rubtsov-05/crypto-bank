using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class Register
{
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        public Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Response(default)); //todo implement
        }
    }
}
