namespace crypto_bank.WebAPI.Features.Accounts.Models;

public record AccountModel(string Number, decimal Amount, string Currency, DateTimeOffset OpenedAt);
