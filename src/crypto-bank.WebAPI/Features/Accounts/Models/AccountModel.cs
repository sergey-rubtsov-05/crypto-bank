namespace crypto_bank.WebAPI.Features.Accounts.Models;

public record AccountModel(long Number, decimal Amount, string Currency, DateTimeOffset OpenedAt);
