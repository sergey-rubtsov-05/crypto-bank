using CryptoBank.WebAPI.Common.Errors.Exceptions.Base;

namespace CryptoBank.WebAPI.Common.Errors.Exceptions;

public class ApiModelValidationException : ErrorException
{
    public ApiModelValidationException(IEnumerable<ProblemDetailsError> errors)
        : this("One or more validation errors have occurred", errors)
    {
    }

    public ApiModelValidationException(string message, IEnumerable<ProblemDetailsError> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IEnumerable<ProblemDetailsError> Errors { get; }
}

public record ProblemDetailsError(string Field, string Message, string Code);
