using crypto_bank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace crypto_bank.WebAPI.Features.Auth.Requests;

public partial class Authenticate
{
    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.Email).NotEmpty();
            RuleFor(request => request.Password).NotEmpty();
        }
    }
}
