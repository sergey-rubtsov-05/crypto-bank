using System.Security.Claims;
using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Domain.Features.Accounts.Models;
using crypto_bank.WebAPI.Errors;
using crypto_bank.WebAPI.Features.Accounts.Errors;
using crypto_bank.WebAPI.Features.Accounts.Options;
using crypto_bank.WebAPI.Validation;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public class Create
{
    public record Request(string Currency) : IRequest<Response>;

    public record Response(long AccountNumber);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.Currency)
                .NotEmpty()
                .Must(currency => string.Equals("BTC", currency, StringComparison.OrdinalIgnoreCase))
                .WithMessage("Only BTC currency is supported");
        }
    }

    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly IClock _clock;
        private readonly CryptoBankDbContext _dbContext;
        private readonly HttpContext? _httpContext;
        private readonly IOptions<AccountsOptions> _options;

        public RequestHandler(
            IClock clock,
            CryptoBankDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountsOptions> options)
        {
            _clock = clock;
            _dbContext = dbContext;
            _options = options;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            //TODO refactor getting user id
            var nameIdentifierValue = _httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = string.IsNullOrWhiteSpace(nameIdentifierValue) ? 0 : int.Parse(nameIdentifierValue);

            var currencyNumberOfAccounts =
                await _dbContext.Accounts.CountAsync(account => account.UserId == userId, cancellationToken);
            if (currencyNumberOfAccounts >= _options.Value.MaximumAccountsPerUser)
                throw new LogicConflictException(CreateAccountLogicConflictErrors.MaximumNumberOfAccountsReached);

            var account = new Account { UserId = userId, Currency = request.Currency, OpenedAt = _clock.UtcNow };

            await _dbContext.Accounts.AddAsync(account, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(account.Number);
        }
    }
}