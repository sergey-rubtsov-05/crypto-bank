using crypto_bank.Database;
using crypto_bank.Domain.Models;
using FluentValidation;

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
        await _dbContext.SaveChangesAsync();

        return entityEntry.Entity;
    }
}
