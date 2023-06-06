namespace CryptoBank.Domain.Models;

public record Token(string RefreshToken, int UserId, DateTimeOffset ExpirationTime)
{
    public User User { get; init; } = null!;
    public bool IsRevoked { get; init; }
    public string? ReplacedById { get; set; }
    public Token? ReplacedBy { get; set; }
}
