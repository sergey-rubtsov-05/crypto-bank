using System.Security.Claims;
using crypto_bank.Database;
using crypto_bank.WebAPI.Features.Accounts.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public class GetList
{
    public record Request(ClaimsPrincipal Principal) : IRequest<Response>;

    public record Response(IReadOnlyList<AccountModel> Accounts);

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
            //TODO refactor getting user id
            var userId = int.Parse(request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var accounts = await _dbContext.Accounts
                .Where(account => account.UserId == userId)
                .Select(account => new AccountModel(account.Number, account.Amount, account.Currency, account.OpenedAt))
                .ToListAsync(cancellationToken);

            return new Response(accounts);
        }
    }
}
