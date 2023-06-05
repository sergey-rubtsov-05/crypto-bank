using crypto_bank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace crypto_bank.WebAPI.Features.Accounts.Requests;

public partial class GetNumberOfOpenedAccounts
{
    public record Request(DateOnly BeginDate, DateOnly EndDate) : IRequest<Response>;

    public record Response(Dictionary<DateOnly, int> report);

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.BeginDate)
                .NotEmpty()
                .LessThan(request => request.EndDate);
        }
    }
}
