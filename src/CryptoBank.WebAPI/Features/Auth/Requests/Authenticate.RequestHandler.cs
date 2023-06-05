using CryptoBank.Database;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Auth.Exceptions;
using CryptoBank.WebAPI.Features.Auth.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebAPI.Features.Auth.Requests;

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
                .Select(user => new { user.Id, user.PasswordHash, user.Roles })
                .SingleOrDefaultAsync(cancellationToken);

            if (userAuthenticateInfo is null)
                throw new AuthenticationException("Couldn't get the user");

            if (!_passwordHasher.Verify(userAuthenticateInfo.PasswordHash, password))
                throw new AuthenticationException("Invalid password");

            return _tokenService.CreateAccessToken(userAuthenticateInfo.Id, userAuthenticateInfo.Roles);
        }
    }
}
