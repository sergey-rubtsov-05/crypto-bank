using crypto_bank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public partial class Create
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
}
