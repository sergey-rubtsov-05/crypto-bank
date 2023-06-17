namespace CryptoBank.WebAPI.Common.Errors;

public abstract class ValidationError
{
    protected ValidationError(string code)
    {
        Code = code;
    }

    public string Code { get; }
}
