using System.Text.Json.Serialization;
using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Common.Validation;
using CryptoBank.WebAPI.Features.Auth.Errors;
using CryptoBank.WebAPI.Features.Auth.Exceptions;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Auth.Services;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

public static class Authenticate
{
    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken, [property: JsonIgnore] string RefreshToken);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.Email).NotEmpty().WithError(AuthValidationError.EmailIsEmpty);
            RuleFor(request => request.Password).NotEmpty().WithError(AuthValidationError.PasswordIsEmpty);
        }
    }

    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AuthOptions _authOptions;
        private readonly AuthService _authService;
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly TokenService _tokenService;

        public RequestHandler(
            IOptions<AuthOptions> authOptions,
            AuthService authService,
            IClock clock,
            CryptoBankDbContext dbContext,
            TokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _authOptions = authOptions.Value;
            _authService = authService;
            _clock = clock;
            _dbContext = dbContext;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
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
            await _authService.Store(refreshToken, userId, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        private async Task RemovePrevious(int userId, CancellationToken cancellationToken)
        {
            await _dbContext.Tokens
                .Where(token => token.UserId == userId)
                .Where(token => token.CreatedAt <= _clock.UtcNow.Add(_authOptions.RefreshTokenArchiveTime))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
