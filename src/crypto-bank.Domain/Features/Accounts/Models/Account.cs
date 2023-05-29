using crypto_bank.Domain.Features.Users.Models;

namespace crypto_bank.Domain.Features.Accounts.Models;

public class Account
{
    public long Number { get; init; }
    public string Currency { get; init; } = null!;
    public decimal Amount { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
    public int UserId { get; init; }
    public User User { get; init; } = null!;
}
