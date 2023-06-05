namespace CryptoBank.WebAPI.Common.Errors.Exceptions.Base;

public abstract class ErrorException : Exception
{
    public ErrorException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
