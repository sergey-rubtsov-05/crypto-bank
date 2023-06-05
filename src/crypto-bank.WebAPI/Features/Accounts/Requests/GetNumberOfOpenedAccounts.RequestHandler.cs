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
            var beginDateTime = request.BeginDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endDateTime = request.EndDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var report = await _dbContext.Accounts
                .Select(account => new { account.Number, account.OpenedAt })
                .Where(account => account.OpenedAt >= beginDateTime && account.OpenedAt <= endDateTime)
                .GroupBy(account => account.OpenedAt.Date)
                .Select(group => new { Date = DateOnly.FromDateTime(group.Key), Count = group.Count() })
                .OrderBy(record => record.Date)
                .ToDictionaryAsync(record => record.Date, record => record.Count, cancellationToken);

            return new Response(report);
        }
    }
}
