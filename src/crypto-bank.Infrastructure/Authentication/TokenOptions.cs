namespace crypto_bank.Infrastructure;

public class TokenOptions
{
    public TimeSpan AccessTokenLifeTime { get; init; }
    public TimeSpan RefreshTokenLifeTime { get; init; }
}
