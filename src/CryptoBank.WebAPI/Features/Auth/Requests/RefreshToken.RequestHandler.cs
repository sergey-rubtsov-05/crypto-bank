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
        private readonly AuthService _authService;
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;
        private readonly TokenService _tokenService;

        public RequestHandler(
            AuthService authService,
            CryptoBankDbContext dbContext,
            IClock clock,
            TokenService tokenService,
            IOptions<AuthOptions> authOptions)
        {
            _authService = authService;
            _dbContext = dbContext;
            _clock = clock;
            _tokenService = tokenService;
            _authOptions = authOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var currentRefreshTokenEntity = await _dbContext.Tokens.SingleOrDefaultAsync(
                token => token.RefreshToken == request.RefreshToken,
                cancellationToken);

            await Verify(currentRefreshTokenEntity, cancellationToken);

            var (newAccessToken, newRefreshToken) = await GenerateNewPair(
                currentRefreshTokenEntity.RefreshToken,
                currentRefreshTokenEntity.UserId,
                cancellationToken);

            return new Response(newAccessToken, newRefreshToken);
        }

        private async Task<(string newAccessToken, string newRefreshToken)> GenerateNewPair(
            string currentRefreshToken,
            int userId,
            CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Where(user => user.Id == userId)
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
            string currentRefreshToken,
            string newRefreshToken,
            int userId,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await RemoveArchived(currentRefreshToken, userId, cancellationToken);
            await _authService.Store(newRefreshToken, userId, cancellationToken);
            await SetReplaceBy(currentRefreshToken, newRefreshToken, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        private async Task RemoveArchived(string currentRefreshToken, int userId, CancellationToken cancellationToken)
        {
            await _dbContext.Tokens
                .Where(token => token.UserId == userId)
                .Where(token => token.RefreshToken != currentRefreshToken)
                .Where(token => token.CreatedAt <= _clock.UtcNow.Add(_authOptions.RefreshTokenArchiveTime))
                .ExecuteDeleteAsync(cancellationToken);
        }

        private async Task SetReplaceBy(
            string currentRefreshToken,
            string newRefreshToken,
            CancellationToken cancellationToken)
        {
            await _dbContext.Tokens
                .Where(token => token.RefreshToken == currentRefreshToken)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.ReplacedById, newRefreshToken), cancellationToken);
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
