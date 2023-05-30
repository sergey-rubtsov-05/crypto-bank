using crypto_bank.Database;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public partial class GetNumberOfOpenedAccounts
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
            var numberOfAccounts = await _dbContext.Accounts
                .Where(account => account.OpenedAt >= request.Begin && account.OpenedAt <= request.End)
                .CountAsync(cancellationToken);

            return new Response(numberOfAccounts);
        }
    }
}
