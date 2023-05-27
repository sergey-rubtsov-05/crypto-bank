namespace crypto_bank.Domain.Features.Users.Models;

public record User
(
    string Email,
    string Password,
    DateOnly? BirthDate,
    DateTimeOffset RegisteredAt) //todo: password as plain text is not secure
{
    public int Id { get; init; }
}
