using System.Runtime.Serialization;
using FluentValidation;
using FluentValidation.Results;

namespace crypto_bank.Domain.Models.Validators.Base;

public class DomainModelValidationException : ValidationException
{
    public DomainModelValidationException(string message)
        : base(message)
    {
    }

    public DomainModelValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message, errors)
    {
    }

    public DomainModelValidationException(
        string message,
        IEnumerable<ValidationFailure> errors,
        bool appendDefaultMessage)
        : base(message, errors, appendDefaultMessage)
    {
    }

    public DomainModelValidationException(IEnumerable<ValidationFailure> errors)
        : base(errors)
    {
    }

    public DomainModelValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
