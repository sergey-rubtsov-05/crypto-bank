using crypto_bank.Domain.Authorization;

namespace crypto_bank.Domain.Features.Users.Models;

public record User
(
    string Email,
    string PasswordHash,
    string Salt,
    DateOnly? BirthDate,
    DateTimeOffset RegisteredAt,
    Role[] Roles)
{
    public int Id { get; init; }
}
