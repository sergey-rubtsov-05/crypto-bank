namespace crypto_bank.WebAPI.Models;

public record UserRegistrationRequest(string Email, string Password); //todo password as plain text, should be hashed
