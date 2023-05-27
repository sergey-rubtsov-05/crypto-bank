namespace crypto_bank.Common;

public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}
