using crypto_bank.Database;
using crypto_bank.Domain.Models;
using crypto_bank.Infrastructure.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Infrastructure;

public class UserService
{
    private readonly CryptoBankDbContext _dbContext;
    private readonly IValidator<User> _userValidator;

    public UserService(CryptoBankDbContext dbContext, IValidator<User> userValidator)
    {
        _dbContext = dbContext;
        _userValidator = userValidator;
    }

    public async Task<User> Register(string email, string password)
    {
        var user = new User(email, password);
        await _userValidator.ValidateAndThrowAsync(user);

        var entityEntry = await _dbContext.Users.AddAsync(user);
        //todo validate email is unique
        await _dbContext.SaveChangesAsync();

        return entityEntry.Entity;
    }

    public async Task<User> Get(string email)
    {
        var users = await _dbContext.Users
            .Where(user => user.Email.Equals(email)) //todo solve problem with case sensitivity
            .Take(2)
            .ToListAsync();

        if (!users.Any())
            throw new UserNotFoundException(email);

        if (users.Count > 1)
            throw new MultipleUsersFoundException(email);

        return users.Single();
    }
}
