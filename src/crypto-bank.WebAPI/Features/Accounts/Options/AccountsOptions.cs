namespace crypto_bank.WebAPI.Features.Accounts.Options;

public record AccountsOptions
{
    public int MaximumAccountsPerUser { get; set; }
}
