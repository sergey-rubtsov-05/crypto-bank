namespace crypto_bank.Domain;

public record User //todo: describe with attributes
{
    public int Id { get; init; }
    public string Email { get; init; }
    public string Password { get; init; } //todo: password as plain text is not secure
}
