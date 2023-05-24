namespace crypto_bank.Infrastructure.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
