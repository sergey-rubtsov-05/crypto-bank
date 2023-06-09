using System.Text.Json.Serialization;
using CryptoBank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public partial class Authenticate
{
    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken, [property: JsonIgnore] string RefreshToken);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.Email).NotEmpty(); //todo add status code
            RuleFor(request => request.Password).NotEmpty();
        }
    }
}
