using CryptoBank.WebAPI.Common.Errors.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace CryptoBank.WebAPI.Common.Validation;

public class ApiModelValidator<T> : AbstractValidator<T>
{
    protected override void RaiseValidationException(ValidationContext<T> context, ValidationResult result)
    {
        var problemDetailErrors = result.Errors
            .Select(
                validationFailure => new ProblemDetailsError(
                    validationFailure.PropertyName,
                    validationFailure.ErrorMessage,
                    validationFailure.ErrorCode));

        throw new ApiModelValidationException(problemDetailErrors);
    }
}
