using CryptoBank.WebAPI.Common.Errors.Exceptions.Base;

namespace CryptoBank.WebAPI.Common.Errors.Exceptions;

public class LogicConflictException : ErrorException
{
    public LogicConflictException(LogicConflictError error, Exception? innerException = null)
        : base(error.Code, innerException)
    {
        ErrorCode = error.Code;
    }

    public string ErrorCode { get; }
}
