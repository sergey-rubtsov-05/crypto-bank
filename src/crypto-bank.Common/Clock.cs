namespace crypto_bank.Common;

public class Clock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
