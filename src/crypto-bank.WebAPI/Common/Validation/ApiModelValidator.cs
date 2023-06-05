using crypto_bank.WebAPI.Common.Errors.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace crypto_bank.WebAPI.Common.Validation;

public class ApiModelValidator<T> : AbstractValidator<T>
{
    protected override void RaiseValidationException(ValidationContext<T> context, ValidationResult result)
    {
        throw new ApiModelValidationException(result.Errors);
    }
}
