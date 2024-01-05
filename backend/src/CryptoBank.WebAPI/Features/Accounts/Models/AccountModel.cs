namespace CryptoBank.WebAPI.Features.Accounts.Models;

public record AccountModel(string Number, decimal Amount, string Currency, DateTimeOffset OpenedAt);
