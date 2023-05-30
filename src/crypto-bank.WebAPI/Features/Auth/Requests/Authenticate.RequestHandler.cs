using crypto_bank.Database;
using crypto_bank.WebAPI.Common.Services;
using crypto_bank.WebAPI.Features.Auth.Exceptions;
using crypto_bank.WebAPI.Features.Auth.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.WebAPI.Features.Auth.Requests;

public partial class Authenticate
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly CryptoBankDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly TokenService _tokenService;

        public RequestHandler(CryptoBankDbContext dbContext, TokenService tokenService, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var accessToken = await Authenticate(request.Email, request.Password, cancellationToken);

            return new Response(accessToken);
        }

        private async Task<string> Authenticate(string email, string password, CancellationToken cancellationToken)
        {
            var userAuthenticateInfo = await _dbContext.Users
                .Where(user => user.Email.Equals(email))
                .Select(user => new { user.Id, user.PasswordHash, user.Salt, user.Roles })
                .SingleOrDefaultAsync(cancellationToken);

            if (userAuthenticateInfo is null)
                throw new AuthenticationException("Couldn't get the user");

            var passwordHash = _passwordHasher.Hash(password, userAuthenticateInfo.Salt);

            if (!userAuthenticateInfo.PasswordHash.Equals(passwordHash, StringComparison.Ordinal))
                throw new AuthenticationException("Invalid password");

            return _tokenService.CreateAccessToken(userAuthenticateInfo.Id, userAuthenticateInfo.Roles);
        }
    }
}
