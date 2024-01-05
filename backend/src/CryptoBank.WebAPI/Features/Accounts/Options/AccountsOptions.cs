namespace CryptoBank.WebAPI.Features.Accounts.Options;

public record AccountsOptions
{
    public int MaximumAccountsPerUser { get; init; }
    public string AccountNumberPrefix { get; init; } = null!;
}
