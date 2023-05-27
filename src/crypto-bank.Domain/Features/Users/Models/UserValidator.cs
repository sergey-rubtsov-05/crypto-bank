using crypto_bank.Common;
using crypto_bank.Domain.Validation;
using FluentValidation;

namespace crypto_bank.Domain.Features.Users.Models;

public class UserValidator : DomainModelValidator<User>
{
    public UserValidator(IClock clock)
    {
        RuleFor(user => user.Email).NotEmpty().EmailAddress();
        RuleFor(user => user.Password).NotEmpty();
        RuleFor(user => user.BirthDate).LessThanOrEqualTo(DateOnly.FromDateTime(clock.UtcNow.Date));
        RuleFor(user => user.RegisteredAt).NotEmpty();
    }
}
