using CryptoBank.Database;
using CryptoBank.WebAPI.Common.Validation;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebAPI.Features.Accounts.Requests;

public static class GetNumberOfOpenedAccounts
{
    public record Request(DateOnly BeginDate, DateOnly EndDate) : IRequest<Response>;

    public record Response([property: UsedImplicitly] Dictionary<DateOnly, int> Report);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.BeginDate)
                .NotEmpty()
                .LessThan(request => request.EndDate);
        }
    }

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
