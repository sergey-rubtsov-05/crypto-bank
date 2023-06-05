namespace CryptoBank.WebAPI.Common.Validation;

public class ValidatorRegistrationException : Exception
{
    public ValidatorRegistrationException(string message)
        : base(message)
    {
    }
}
