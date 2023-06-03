using System.Security.Claims;
using System.Security.Cryptography;
using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Domain.Models;
using crypto_bank.WebAPI.Common.Errors.Exceptions;
using crypto_bank.WebAPI.Features.Accounts.Errors;
using crypto_bank.WebAPI.Features.Accounts.Options;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public partial class Create
{
    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;
        private readonly HttpContext? _httpContext;
        private readonly AccountsOptions _options;

        public RequestHandler(
            IClock clock,
            CryptoBankDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountsOptions> options)
        {
            _clock = clock;
            _dbContext = dbContext;
            _options = options.Value;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            //TODO refactor getting user id
            var nameIdentifierValue = _httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = string.IsNullOrWhiteSpace(nameIdentifierValue) ? 0 : int.Parse(nameIdentifierValue);

            await EnsureCanCreateAccount(userId, cancellationToken);

            var account = await AddAccount(userId, request.Currency, cancellationToken);

            return new Response(account.Number);
        }

        private async Task EnsureCanCreateAccount(int userId, CancellationToken cancellationToken)
        {
            var currentNumberOfAccounts =
                await _dbContext.Accounts.CountAsync(account => account.UserId == userId, cancellationToken);

            if (currentNumberOfAccounts >= _options.MaximumAccountsPerUser)
                throw new LogicConflictException(AccountsLogicConflictErrors.MaximumNumberOfAccountsReached);
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
