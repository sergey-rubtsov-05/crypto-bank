using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Errors.Exceptions;
using CryptoBank.WebAPI.Common.Validation;
using CryptoBank.WebAPI.Features.Auth.Errors;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public static class RevokeToken
{
    public record Request(string RefreshToken) : IRequest;

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty()
                .WithError(AuthValidationError.RefreshTokenEmpty);
        }
    }

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
                token => token.RefreshToken == request.RefreshToken,
                cancellationToken);

            Verify(token);

            await Revoke(token.RefreshToken, cancellationToken);
        }

        private void Verify([System.Diagnostics.CodeAnalysis.NotNull] Token? token)
        {
            if (token == null)
                throw new ApiValidationException(AuthValidationError.RefreshTokenDoesNotExist);

            if (token.IsRevoked)
                throw new LogicConflictException(AuthLogicConflictError.RefreshTokenAlreadyRevoked);

            if (_clock.UtcNow >= token.ExpirationTime)
                throw new LogicConflictException(AuthLogicConflictError.RefreshTokenExpired);
        }

        private async Task Revoke(string refreshToken, CancellationToken cancellationToken)
        {
            await _dbContext.Tokens
                .Where(token => token.RefreshToken == refreshToken)
                .ExecuteUpdateAsync(calls => calls.SetProperty(token => token.IsRevoked, true), cancellationToken);
        }
    }
}
