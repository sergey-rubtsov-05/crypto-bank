namespace CryptoBank.WebAPI.Common.Errors;

public abstract class LogicConflictError
{
    protected LogicConflictError(string code)
    {
        Code = code;
    }

    public string Code { get; }
}
