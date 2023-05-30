using crypto_bank.Common;
using crypto_bank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public static partial class Register
{
    public record Request(string Email, string Password, DateOnly? BirthDate) : IRequest<Response>;

    public record Response(int Id);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator(IClock clock)
        {
            RuleFor(request => request.Email).NotEmpty().EmailAddress();
            RuleFor(request => request.Password).NotEmpty();
            RuleFor(request => request.BirthDate).LessThanOrEqualTo(DateOnly.FromDateTime(clock.UtcNow.Date));
        }
    }
}
