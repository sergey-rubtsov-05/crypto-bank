using System.Runtime.Serialization;
using FluentValidation;
using FluentValidation.Results;

namespace crypto_bank.WebAPI.Validation;

public class ApiModelValidationException : ValidationException
{
    public ApiModelValidationException(string message)
        : base(message)
    {
    }

    public ApiModelValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message, errors)
    {
    }

    public ApiModelValidationException(string message, IEnumerable<ValidationFailure> errors, bool appendDefaultMessage)
        : base(message, errors, appendDefaultMessage)
    {
    }

    public ApiModelValidationException(IEnumerable<ValidationFailure> errors)
        : base(errors)
    {
    }

    public ApiModelValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
