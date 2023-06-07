using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Errors.Exceptions;
using CryptoBank.WebAPI.Features.Auth.Errors;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public partial class RevokeToken
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request>
    {
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;

        public RequestHandler(CryptoBankDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var token = await _dbContext.Tokens.SingleOrDefaultAsync(
                token => token.RefreshToken == request.RefreshToken, cancellationToken);

            Verify(token);

            await Revoke(token.RefreshToken, cancellationToken);
        }

        private void Verify([System.Diagnostics.CodeAnalysis.NotNull] Token? token)
        {
            if (token == null)
                throw new LogicConflictException(AuthLogicConflictErrorCode.RefreshTokenDoesNotExist);

            if (token.IsRevoked)
                throw new LogicConflictException(AuthLogicConflictErrorCode.RefreshTokenAlreadyRevoked);

            if (_clock.UtcNow >= token.ExpirationTime)
                throw new LogicConflictException(AuthLogicConflictErrorCode.RefreshTokenExpired);
        }

        private async Task Revoke(string refreshToken, CancellationToken cancellationToken)
        {
            await _dbContext.Tokens
                .Where(token => token.RefreshToken == refreshToken)
                .ExecuteUpdateAsync(calls => calls.SetProperty(token => token.IsRevoked, true), cancellationToken);
        }
    }
}
