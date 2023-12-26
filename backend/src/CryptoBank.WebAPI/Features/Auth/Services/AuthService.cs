using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Auth.Options;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebAPI.Features.Auth.Services;

public class AuthService
{
    private readonly AuthOptions _authOptions;
    private readonly IClock _clock;
    private readonly CryptoBankDbContext _dbContext;

    public AuthService(CryptoBankDbContext dbContext, IClock clock, IOptions<AuthOptions> authOptions)
    {
        _dbContext = dbContext;
        _clock = clock;
        _authOptions = authOptions.Value;
    }

    public async Task Store(string refreshToken, int userId, CancellationToken cancellationToken)
    {
        var utcNow = _clock.UtcNow;
        var refreshTokenExpirationTime = utcNow.Add(_authOptions.RefreshTokenLifeTime);

        await _dbContext.Tokens
            .AddAsync(new Token(refreshToken, userId, utcNow, refreshTokenExpirationTime), cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
