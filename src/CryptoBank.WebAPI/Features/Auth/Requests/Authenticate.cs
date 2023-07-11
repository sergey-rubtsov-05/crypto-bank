using System.Text.Json.Serialization;
using CryptoBank.WebAPI.Common.Validation;
using CryptoBank.WebAPI.Features.Auth.Errors;
using FluentValidation;
using MediatR;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public static partial class Authenticate
{
    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken, [property: JsonIgnore] string RefreshToken);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.Email).NotEmpty().WithError(AuthValidationError.EmailIsEmpty);
            RuleFor(request => request.Password).NotEmpty().WithError(AuthValidationError.PasswordIsEmpty);
        }
    }
}
