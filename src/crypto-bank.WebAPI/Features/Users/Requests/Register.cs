using crypto_bank.WebAPI.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Internal;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class Register
{
    public record Request
    (
        string Email,
        string Password,
        DateOnly? BirthDate) : IRequest<Response>; // todo password as plain text, should be hashed

    public record Response(int Id);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator(ISystemClock systemClock)
        {
            RuleFor(request => request.Email).NotEmpty().EmailAddress();
            RuleFor(request => request.Password).NotEmpty();
            RuleFor(request => request.BirthDate).LessThanOrEqualTo(DateOnly.FromDateTime(systemClock.UtcNow.Date));
        }
    }
}
