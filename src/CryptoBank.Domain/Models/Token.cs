namespace CryptoBank.Domain.Models;

public record Token(string RefreshToken, int UserId, DateTimeOffset ExpirationTime)
{
    public User User { get; init; } = null!;
}
