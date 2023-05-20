namespace crypto_bank.Infrastructure.Exceptions;

public class MultipleUsersFoundException : GettingUserException
{
    public MultipleUsersFoundException(string email)
        : base(email)
    {
    }
}
