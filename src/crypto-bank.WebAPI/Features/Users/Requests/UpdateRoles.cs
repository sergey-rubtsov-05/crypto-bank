using crypto_bank.Domain.Authorization;
using crypto_bank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace crypto_bank.WebAPI.Features.Users.Requests;

public partial class UpdateRoles
{
    public record Request(int UserId, Role[] NewRoles) : IRequest;

    public class RequestValidator : ApiModelValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.UserId).NotEmpty();
            RuleFor(request => request.NewRoles).NotEmpty();
        }
    }
}
