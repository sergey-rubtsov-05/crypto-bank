using System.Security.Cryptography;
using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Errors.Exceptions;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Accounts.Errors;
using CryptoBank.WebAPI.Features.Accounts.Options;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebAPI.Features.Accounts.Requests;

public partial class Create
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly IClock _clock;
        private readonly CurrentAuthInfoSource _currentAuthInfoSource;
        private readonly CryptoBankDbContext _dbContext;
        private readonly AccountsOptions _options;

        public RequestHandler(
            IClock clock,
            CryptoBankDbContext dbContext,
            CurrentAuthInfoSource currentAuthInfoSource,
            IOptions<AccountsOptions> options)
        {
            _clock = clock;
            _dbContext = dbContext;
            _currentAuthInfoSource = currentAuthInfoSource;
            _options = options.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var userId = _currentAuthInfoSource.GetUserId();

            await EnsureCanCreateAccount(userId, cancellationToken);

            var account = await AddAccount(userId, request.Currency, cancellationToken);

            return new Response(account.Number);
        }

        private async Task EnsureCanCreateAccount(int userId, CancellationToken cancellationToken)
        {
            var currentNumberOfAccounts =
                await _dbContext.Accounts.CountAsync(account => account.UserId == userId, cancellationToken);

            if (currentNumberOfAccounts >= _options.MaximumAccountsPerUser)
                throw new LogicConflictException(AccountsLogicConflictError.MaximumNumberOfAccountsReached);
        }

        private async Task<Account> AddAccount(int userId, string currency, CancellationToken cancellationToken)
        {
            var accountNumber = GenerateRandomAccountNumber();

            var account = new Account(accountNumber)
            {
                UserId = userId, Currency = currency, OpenedAt = _clock.UtcNow,
            };

            await _dbContext.Accounts.AddAsync(account, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return account;
        }

        private string GenerateRandomAccountNumber()
        {
            var randomAccountId = RandomNumberGenerator.GetInt32(1, 1_000_000_000);
            var accountNumber = $"{_options.AccountNumberPrefix}{randomAccountId:D9}";
            return accountNumber;
        }
    }
}
