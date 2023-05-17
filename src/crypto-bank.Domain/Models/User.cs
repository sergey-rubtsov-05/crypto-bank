namespace crypto_bank.Domain.Models;

public record User(string Email, string Password) //todo: password as plain text is not secure
{
    public int Id { get; init; }
}
