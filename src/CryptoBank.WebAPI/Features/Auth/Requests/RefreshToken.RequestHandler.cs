using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Auth.Exceptions;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Auth.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public partial class RefreshToken
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AuthOptions _authOptions;
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;
        private readonly TokenService _tokenService;

        public RequestHandler(
            CryptoBankDbContext dbContext,
            IClock clock,
            TokenService tokenService,
            IOptions<AuthOptions> authOptions)
        {
            _dbContext = dbContext;
            _clock = clock;
            _tokenService = tokenService;
            _authOptions = authOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var token = await _dbContext.Tokens.SingleOrDefaultAsync(
                token => token.RefreshToken == request.RefreshToken, cancellationToken);

            await Verify(token, cancellationToken);

            var (newAccessToken, newRefreshToken) = await GenerateNewPair(token, cancellationToken);

            return new Response(newAccessToken, newRefreshToken);
        }

        private async Task<(string newAccessToken, string newRefreshToken)> GenerateNewPair(
            Token currentRefreshToken,
            CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Where(user => user.Id == currentRefreshToken.UserId)
                .Select(user => new { user.Id, user.Roles })
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
                throw new AuthenticationException("Could not get the user");

            var newAccessToken = _tokenService.CreateAccessToken(user.Id, user.Roles);
            var newRefreshToken = _tokenService.CreateRefreshToken();

            await RemoveArchivedAndStore(currentRefreshToken, newRefreshToken, user.Id, cancellationToken);

            return (newAccessToken, newRefreshToken);
        }

        private async Task RemoveArchivedAndStore(
            Token currentRefreshToken,
            string newRefreshToken,
            int userId,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await RemoveArchived(currentRefreshToken, cancellationToken);
            await Store(currentRefreshToken, newRefreshToken, userId, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        private async Task RemoveArchived(Token currentRefreshToken, CancellationToken cancellationToken)
        {
            await _dbContext.Tokens
                .Where(token => token.UserId == currentRefreshToken.UserId)
                .Where(token => token.RefreshToken != currentRefreshToken.RefreshToken)
                .Where(token => token.CreatedAt <= _clock.UtcNow.Add(_authOptions.RefreshTokenArchiveTime))
                .ExecuteDeleteAsync(cancellationToken);
        }

        private async Task Store(
            Token currentRefreshTokenEntity,
            string newRefreshToken,
            int userId,
            CancellationToken cancellationToken)
        {
            //todo: DRY problem, I have the same code in Authenticate
            var utcNow = _clock.UtcNow;
            var expirationTime = utcNow.Add(_authOptions.RefreshTokenLifeTime);
            var newRefreshTokenEntity = new Token(newRefreshToken, userId, utcNow, expirationTime);
            _dbContext.Tokens.Add(newRefreshTokenEntity);

            //todo: question: what if a user refresh token every 5 minutes for a day, it is 288 tokens, do we need them all? 
            currentRefreshTokenEntity.ReplacedById = newRefreshToken;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task Verify(
            [System.Diagnostics.CodeAnalysis.NotNull] Token? token,
            CancellationToken cancellationToken)
        {
            if (token == null)
                throw new AuthenticationException("Could not find the refresh token");

            if (token.IsRevoked)
            {
                await RevokeAllDescendantTokens(token, cancellationToken);
                throw new AuthenticationException("The refresh token has been revoked");
            }

            if (_clock.UtcNow >= token.ExpirationTime)
                throw new AuthenticationException("The refresh token has expired");
        }

        private async Task RevokeAllDescendantTokens(Token revokedToken, CancellationToken cancellationToken)
        {
            //todo: question: when user successfully authenticate we remove all previous tokens of this user, so we lost all revoked tokens.
            await _dbContext.Tokens
                .Where(token => token.UserId == revokedToken.UserId)
                .ExecuteUpdateAsync(token => token.SetProperty(t => t.IsRevoked, true), cancellationToken);
        }
    }
}
