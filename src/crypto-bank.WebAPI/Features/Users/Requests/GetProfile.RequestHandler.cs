using System.Security.Claims;
using crypto_bank.Database;
using crypto_bank.WebAPI.Common.Errors.Exceptions;
using crypto_bank.WebAPI.Features.Users.Errors;
using crypto_bank.WebAPI.Features.Users.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class GetProfile
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly CryptoBankDbContext _dbContext;

        public RequestHandler(CryptoBankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var nameIdentifierValue = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = string.IsNullOrWhiteSpace(nameIdentifierValue) ? 0 : int.Parse(nameIdentifierValue);
            var userModel = await _dbContext.Users
                .Where(user => user.Id == userId)
                .Select(user => new UserModel(user.Id, user.Email, user.BirthDate, user.RegisteredAt, user.Roles))
                .SingleOrDefaultAsync(cancellationToken);

            if (userModel is null)
                throw new LogicConflictException(UsersLogicConflictError.UserNotFound);

            return new Response(userModel);
        }
    }
}
