using System.Text.Json.Serialization;
using CryptoBank.WebAPI.Common.Validation;
using CryptoBank.WebAPI.Features.Auth.Errors;
using FluentValidation;
using MediatR;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public partial class RefreshToken
{
    public record Request(string? RefreshToken) : IRequest<Response>;

    public record Response(string AccessToken, [property: JsonIgnore] string RefreshToken);

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
