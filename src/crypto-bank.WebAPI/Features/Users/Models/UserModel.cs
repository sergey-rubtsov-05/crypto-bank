using crypto_bank.Domain.Authorization;

namespace crypto_bank.WebAPI.Features.Users.Models;

public record UserModel(int Id, string Email, DateOnly? BirthDate, DateTimeOffset RegisteredAt, PolicyNames Roles);
