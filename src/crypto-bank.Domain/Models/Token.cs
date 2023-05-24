namespace crypto_bank.Domain.Models;

public record Token(Guid Id, string AccessToken, string RefreshToken);
