namespace crypto_bank.WebAPI.Models;

public record UserLoginRequest(string Email, string Password); //todo password as plain text, should be hashed
