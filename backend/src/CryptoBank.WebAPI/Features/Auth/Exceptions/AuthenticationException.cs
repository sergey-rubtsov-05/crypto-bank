namespace CryptoBank.WebAPI.Features.Auth.Exceptions;

public class AuthenticationException : Exception
{
    public AuthenticationException(string message)
        : base(message)
    {
    }
}
