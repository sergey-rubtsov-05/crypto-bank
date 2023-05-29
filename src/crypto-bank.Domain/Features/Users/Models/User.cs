using crypto_bank.Domain.Authorization;

namespace crypto_bank.Domain.Features.Users.Models;

public record User
{
    public User(int id)
    {
        Id = id;
    }

    public User(
        string Email,
        string PasswordHash,
        string Salt,
        DateOnly? BirthDate,
        DateTimeOffset RegisteredAt,
        Role[] Roles)
    {
        this.Email = Email;
        this.PasswordHash = PasswordHash;
        this.Salt = Salt;
        this.BirthDate = BirthDate;
        this.RegisteredAt = RegisteredAt;
        this.Roles = Roles;
    }

    public int Id { get; init; }
    public string Email { get; init; } = null!; // todo: question: is it ok to suppress nullable warning here?
    public string PasswordHash { get; init; } = null!;
    public string Salt { get; init; } = null!;
    public DateOnly? BirthDate { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
    public Role[] Roles { get; init; } = null!;
}
