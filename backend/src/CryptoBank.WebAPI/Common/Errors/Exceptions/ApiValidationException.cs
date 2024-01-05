using CryptoBank.WebAPI.Common.Errors.Exceptions.Base;

namespace CryptoBank.WebAPI.Common.Errors.Exceptions;

public class ApiValidationException : ErrorException
{
    public ApiValidationException(ValidationError error)
    {
        ErrorCode = error.Code;
    }

    public string ErrorCode { get; }
}
