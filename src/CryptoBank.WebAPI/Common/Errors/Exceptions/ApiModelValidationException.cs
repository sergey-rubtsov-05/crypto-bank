using CryptoBank.WebAPI.Common.Errors.Exceptions.Base;
using FluentValidation.Results;

namespace CryptoBank.WebAPI.Common.Errors.Exceptions;

public class ApiModelValidationException : ErrorException
{
    public ApiModelValidationException(IEnumerable<ValidationFailure> errors)
        : this("One or more validation errors have occurred", errors)
    {
    }

    public ApiModelValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IEnumerable<ValidationFailure> Errors { get; private set; }
}
