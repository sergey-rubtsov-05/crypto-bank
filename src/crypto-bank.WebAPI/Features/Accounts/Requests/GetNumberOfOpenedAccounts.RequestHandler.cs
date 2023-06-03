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
            //TODO BUG? If we use IQueryable.GroupBy() we get all days grouped in one group (as the min date in DB)
            // It will be a problem when we will have a lot of accounts and we want to group them by date
            var accounts = await _dbContext.Accounts
                .Select(account => new { account.Number, OpenedAtDate = DateOnly.FromDateTime(account.OpenedAt.Date) })
                .Where(account => account.OpenedAtDate >= request.BeginDate && account.OpenedAtDate <= request.EndDate)
                .ToArrayAsync(cancellationToken);

            var report = accounts
                .GroupBy(account => account.OpenedAtDate)
                .OrderBy(group => group.Key)
                .ToDictionary(
                    groupedAccounts => groupedAccounts.Key,
                    groupedAccounts => groupedAccounts.Count());

            return new Response(report);
        }
    }
}
