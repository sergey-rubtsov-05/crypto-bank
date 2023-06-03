namespace crypto_bank.Domain.Models;

public record Account
{
    public Account(string number)
    {
        Number = number;
    }

    public string Number { get; init; }
    public string Currency { get; init; } = null!;
    public decimal Amount { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
    public int UserId { get; init; }
    public User User { get; init; } = null!;
}
