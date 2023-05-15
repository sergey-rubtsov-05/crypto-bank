namespace crypto_bank.WebAPI.Models;

public record UserRegistrationRequest
{
    public string Email { get; init; }
    public string Password { get; init; } //todo password is in plain text, should be hashed
}
