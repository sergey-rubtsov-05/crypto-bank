using CryptoBank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace CryptoBank.WebAPI.Features.Accounts.Requests;

public partial class Create
{
    public record Request(string Currency) : IRequest<Response>;

    public record Response(string AccountNumber);

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
