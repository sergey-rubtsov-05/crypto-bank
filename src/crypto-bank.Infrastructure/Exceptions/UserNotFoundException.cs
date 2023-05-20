namespace crypto_bank.Infrastructure.Exceptions;

public class UserNotFoundException : GettingUserException
{
    public UserNotFoundException(string email)
        : base(email)
    {
    }
}
