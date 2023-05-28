using crypto_bank.Database;
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
            var userModel = await _dbContext.Users
                .Select(user => new UserModel(user.Id, user.Email, user.BirthDate, user.RegisteredAt, user.Roles))
                .FirstAsync(cancellationToken);

            return new Response(userModel);
        }
    }
}
