namespace crypto_bank.WebAPI.Errors;

public class LogicConflictException : Exception
{
    public LogicConflictException(string code, Exception? innerException = null)
        : base(code, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
