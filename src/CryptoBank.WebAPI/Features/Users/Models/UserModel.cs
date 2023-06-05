using CryptoBank.Domain.Authorization;

namespace CryptoBank.WebAPI.Features.Users.Models;

public record UserModel(int Id, string Email, DateOnly? BirthDate, DateTimeOffset RegisteredAt, Role[] Roles);
