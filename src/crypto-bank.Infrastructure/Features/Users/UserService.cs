using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Domain.Authorization;
using crypto_bank.Domain.Features.Users.Models;
using crypto_bank.Infrastructure.Common;
using crypto_bank.Infrastructure.Features.Users.Exceptions;
using crypto_bank.Infrastructure.Features.Users.Options;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace crypto_bank.Infrastructure.Features.Users;

public class UserService
{
    private readonly IClock _clock;
    private readonly CryptoBankDbContext _dbContext;
    private readonly UsersOptions _options;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<User> _userValidator;

    public UserService(
        CryptoBankDbContext dbContext,
        IValidator<User> userValidator,
        IClock clock,
        IPasswordHasher passwordHasher,
        IOptions<UsersOptions> options)
    {
        _dbContext = dbContext;
        _userValidator = userValidator;
        _clock = clock;
        _passwordHasher = passwordHasher;
        _options = options.Value;
    }

    public async Task<User> Register(string email, string password, DateOnly? birthDate = null)
    {
        var salt = Guid.NewGuid().ToString();
        var passwordHash = _passwordHasher.Hash(password, salt);

        var roles = await DetermineRoles(email);

        var user = new User(email, passwordHash, salt, birthDate, _clock.UtcNow, roles);
        await _userValidator.ValidateAndThrowAsync(user);

        await ValidateUserExistingAndThrow(email);

        var entityEntry = await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return entityEntry.Entity;
    }

    private async Task<Role[]> DetermineRoles(string email)
    {
        var administratorAlreadyExits =
            await _dbContext.Users.AnyAsync(user => user.Roles.Contains(Role.Administrator));

        if (administratorAlreadyExits)
            return new[] { Role.User };

        var isInitialAdministratorEmail = email.Equals(_options.AdministratorEmail, StringComparison.OrdinalIgnoreCase);

        if (isInitialAdministratorEmail)
            return new[] { Role.Administrator };

        return new[] { Role.User };
    }

    private async Task ValidateUserExistingAndThrow(string email)
    {
        var isExist = await _dbContext.Users.AnyAsync(user => user.Email.Equals(email));
        if (isExist)
            throw new UserAlreadyExistsException(email);
    }

    public async Task UpdateRoles(int userId, Role[] newRoles)
    {
        var updatedUser = new User(userId) { Roles = newRoles };

        _dbContext.Attach(updatedUser);
        _dbContext.Entry(updatedUser).Property(user => user.Roles).IsModified = true;

        await _dbContext.SaveChangesAsync();
    }
}