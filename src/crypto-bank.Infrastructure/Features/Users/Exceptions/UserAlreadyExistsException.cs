namespace crypto_bank.Infrastructure.Features.Users.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
