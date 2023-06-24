namespace CryptoBank.WebAPI.Features.Auth.Options;

public record AuthOptions
{
    public JwtOptions Jwt { get; init; } = default!;
    public TimeSpan RefreshTokenLifeTime { get; init; }
    public TimeSpan RefreshTokenArchiveTime { get; init; }
}

public record JwtOptions
{
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public TimeSpan AccessTokenLifeTime { get; init; }
    public string SigningKey { get; init; } = default!; //todo get from secrets, not from config
}
