using CryptoBank.Database;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Accounts.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebAPI.Features.Accounts.Requests;

public partial class GetList
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

            var accounts = await _dbContext.Accounts
                .Where(account => account.UserId == userId)
                .Select(account => new AccountModel(account.Number, account.Amount, account.Currency, account.OpenedAt))
                .ToListAsync(cancellationToken);

            return new Response(accounts);
        }
    }
}
