using crypto_bank.Domain.Models.Validators.Base;
using FluentValidation;

namespace crypto_bank.Domain.Models.Validators;

public class UserValidator : DomainModelValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.Email).NotEmpty().EmailAddress();
        RuleFor(user => user.Password).NotEmpty();
    }
}
