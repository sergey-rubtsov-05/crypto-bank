using crypto_bank.Database;
using crypto_bank.Domain.Features.Users.Models;
using JetBrains.Annotations;
using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class UpdateRoles
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request>
    {
        private readonly CryptoBankDbContext _dbContext;

        public RequestHandler(CryptoBankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var updatedUser = new User(request.UserId) { Roles = request.NewRoles };

            _dbContext.Attach(updatedUser);
            _dbContext.Entry(updatedUser).Property(user => user.Roles).IsModified = true;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
