using FluentValidation;
using FluentValidation.Results;

namespace crypto_bank.Domain.Validation;

public class DomainModelValidator<T> : AbstractValidator<T>
{
    protected override void RaiseValidationException(ValidationContext<T> context, ValidationResult result)
    {
        throw new DomainModelValidationException($"Domain model [{typeof(T).Name}] validation failed", result.Errors);
    }
}
