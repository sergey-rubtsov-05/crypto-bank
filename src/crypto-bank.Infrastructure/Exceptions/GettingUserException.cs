namespace crypto_bank.Infrastructure.Exceptions;

public class GettingUserException : Exception
{
    public GettingUserException(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
