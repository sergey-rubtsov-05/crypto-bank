using crypto_bank.WebAPI.Models.Validators.Base;
using FluentValidation;

namespace crypto_bank.WebAPI.Models.Validators;

public class UserRegistrationRequestValidator : ApiModelValidator<UserRegistrationRequest>
{
    public UserRegistrationRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password).NotEmpty();
    }
}
