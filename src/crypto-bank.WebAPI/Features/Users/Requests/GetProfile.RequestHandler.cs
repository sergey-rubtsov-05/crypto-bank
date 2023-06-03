using crypto_bank.Database;
using crypto_bank.WebAPI.Common.Errors.Exceptions;
using crypto_bank.WebAPI.Common.Services;
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
        private readonly CurrentAuthInfoSource _currentAuthInfoSource;
        private readonly CryptoBankDbContext _dbContext;

        public RequestHandler(CryptoBankDbContext dbContext, CurrentAuthInfoSource currentAuthInfoSource)
        {
            _dbContext = dbContext;
            _currentAuthInfoSource = currentAuthInfoSource;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var userId = _currentAuthInfoSource.GetUserId();
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
