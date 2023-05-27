using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Domain.Features.Users.Models;
using crypto_bank.Infrastructure.Features.Users.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Infrastructure.Features.Users;

public class UserService
{
    private readonly IClock _clock;
    private readonly CryptoBankDbContext _dbContext;
    private readonly IValidator<User> _userValidator;

    public UserService(CryptoBankDbContext dbContext, IValidator<User> userValidator, IClock clock)
    {
        _dbContext = dbContext;
        _userValidator = userValidator;
        _clock = clock;
    }

    public async Task<User> Register(string email, string password, DateOnly? birthDate = null)
    {
        var user = new User(email, password, birthDate, _clock.UtcNow);
        await _userValidator.ValidateAndThrowAsync(user);

        await ValidateUserExistingAndThrow(email);

        var entityEntry = await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return entityEntry.Entity;
    }

    public async Task<User> Get(string email)
    {
        //todo solve problem with case sensitivity
        var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Email.Equals(email));

        if (user is null)
            throw new UserNotFoundException(email);

        return user;
    }

    private async Task ValidateUserExistingAndThrow(string email)
    {
        var isExist = await _dbContext.Users.AnyAsync(user => user.Email.Equals(email));
        if (isExist)
            throw new UserAlreadyExistsException(email);
    }
}
