namespace crypto_bank.Common;

public interface IClock
{
    public DateTime UtcNow { get; }
}
