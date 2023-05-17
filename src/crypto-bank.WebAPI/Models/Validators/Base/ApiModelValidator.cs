using FluentValidation;
using FluentValidation.Results;

namespace crypto_bank.WebAPI.Models.Validators.Base;

public class ApiModelValidator<T> : AbstractValidator<T>
{
    protected override void RaiseValidationException(ValidationContext<T> context, ValidationResult result)
    {
        throw new ApiModelValidationException(result.Errors);
    }
}
