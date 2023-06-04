using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Auth.Exceptions;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Auth.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public partial class Authenticate
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AuthOptions _authOptions;
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly TokenService _tokenService;

        public RequestHandler(
            CryptoBankDbContext dbContext,
            TokenService tokenService,
            IPasswordHasher passwordHasher,
            IClock clock,
            IOptions<AuthOptions> authOptions)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _clock = clock;
            _authOptions = authOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var (userId, roles) = await GetVerifiedUserInfo(request.Email, request.Password, cancellationToken);

            var accessToken = _tokenService.CreateAccessToken(userId, roles);
            var refreshToken = _tokenService.CreateRefreshToken();

            await RemovePreviousAndStore(refreshToken, userId, cancellationToken);

            return new Response(accessToken, refreshToken);
        }

        private async Task<(int UserId, Role[] Roles)> GetVerifiedUserInfo(
            string email,
            string providerPassword,
            CancellationToken cancellationToken)
        {
            var userAuthenticateInfo = await _dbContext.Users
                .Where(user => user.Email.Equals(email))
                .Select(user => new { user.Id, user.PasswordHash, user.Roles })
                .SingleOrDefaultAsync(cancellationToken);

            if (userAuthenticateInfo is null)
                throw new AuthenticationException("Couldn't get the user");

            if (!_passwordHasher.Verify(userAuthenticateInfo.PasswordHash, providerPassword))
                throw new AuthenticationException("Invalid password");

            return (userAuthenticateInfo.Id, userAuthenticateInfo.Roles);
        }

        private async Task RemovePreviousAndStore(string refreshToken, int userId, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await RemovePrevious(userId, cancellationToken);
            await Store(refreshToken, userId, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        private async Task RemovePrevious(int userId, CancellationToken cancellationToken)
        {
            await _dbContext.Tokens.Where(token => token.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        }

        private async Task Store(string refreshToken, int userId, CancellationToken cancellationToken)
        {
            var refreshTokenExpirationTime = _clock.UtcNow.Add(_authOptions.RefreshTokenLifeTime);
            await _dbContext.Tokens
                .AddAsync(new Token(refreshToken, userId, refreshTokenExpirationTime), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
