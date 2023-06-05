using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Errors.Exceptions;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Users.Errors;
using CryptoBank.WebAPI.Features.Users.Options;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebAPI.Features.Users.Requests;

public partial class Register
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;
        private readonly UsersOptions _options;
        private readonly IPasswordHasher _passwordHasher;

        public RequestHandler(
            IPasswordHasher passwordHasher,
            CryptoBankDbContext dbContext,
            IClock clock,
            IOptions<UsersOptions> options)
        {
            _passwordHasher = passwordHasher;
            _dbContext = dbContext;
            _clock = clock;
            _options = options.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await Register(request.Email, request.Password, request.BirthDate, cancellationToken);

            return new Response(user.Id);
        }

        private async Task<User> Register(
            string email,
            string password,
            DateOnly? birthDate,
            CancellationToken cancellationToken)
        {
            var passwordHash = _passwordHasher.Hash(password);

            var roles = await DetermineRoles(email);

            var user = new User(email, passwordHash, birthDate, _clock.UtcNow, roles);

            await ValidateUserExistingAndThrow(email, cancellationToken);

            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return user;
        }

        private async Task<Role[]> DetermineRoles(string email)
        {
            var administratorAlreadyExits =
                await _dbContext.Users.AnyAsync(user => user.Roles.Contains(Role.Administrator));

            if (administratorAlreadyExits)
                return new[] { Role.User };

            var isInitialAdministratorEmail =
                email.Equals(_options.AdministratorEmail, StringComparison.OrdinalIgnoreCase);

            if (isInitialAdministratorEmail)
                return new[] { Role.Administrator };

            return new[] { Role.User };
        }

        private async Task ValidateUserExistingAndThrow(string email, CancellationToken cancellationToken)
        {
            var isExist = await _dbContext.Users.AnyAsync(user => user.Email.Equals(email), cancellationToken);
            if (isExist)
                throw new LogicConflictException(UsersLogicConflictError.EmailAlreadyUse);
        }
    }
}
