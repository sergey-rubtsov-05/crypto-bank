using CryptoBank.WebAPI.Common.Validation;
using CryptoBank.WebAPI.Features.Auth.Errors;
using FluentValidation;
using MediatR;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public partial class RefreshToken
{
    public record Request(string RefreshToken) : IRequest<Response>;

    public record Response(string AccessToken, string RefreshToken);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty()
                .WithErrorCode(AuthLogicConflictErrorCode.RefreshTokenEmpty);
        }
    }
}
