namespace crypto_bank.Common;

public class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
