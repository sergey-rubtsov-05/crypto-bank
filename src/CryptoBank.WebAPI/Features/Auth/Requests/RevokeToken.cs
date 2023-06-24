using CryptoBank.WebAPI.Common.Validation;
using CryptoBank.WebAPI.Features.Auth.Errors;
using FluentValidation;
using MediatR;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public partial class RevokeToken
{
    public record Request(string RefreshToken) : IRequest;

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty()
                .WithError(AuthValidationError.RefreshTokenEmpty);
        }
    }
}
