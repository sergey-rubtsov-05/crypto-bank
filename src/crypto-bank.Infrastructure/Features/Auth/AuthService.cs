using crypto_bank.Database;
using crypto_bank.Infrastructure.Common;
using crypto_bank.Infrastructure.Features.Auth.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Infrastructure.Features.Auth;

public class AuthService
{
    private readonly CryptoBankDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthService(CryptoBankDbContext dbContext, TokenService tokenService, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> Authenticate(string email, string password)
    {
        var userAuthenticateInfo = await _dbContext.Users
            .Where(user => user.Email.Equals(email))
            .Select(user => new { user.Id, user.PasswordHash, user.Salt, user.Roles })
            .SingleOrDefaultAsync();

        if (userAuthenticateInfo is null)
            throw new AuthenticationException("Couldn't get the user");

        var passwordHash = _passwordHasher.Hash(password, userAuthenticateInfo.Salt);

        if (!userAuthenticateInfo.PasswordHash.Equals(passwordHash, StringComparison.Ordinal))
            throw new AuthenticationException("Invalid password");

        return _tokenService.CreateAccessToken(userAuthenticateInfo.Id, userAuthenticateInfo.Roles);
    }
}
