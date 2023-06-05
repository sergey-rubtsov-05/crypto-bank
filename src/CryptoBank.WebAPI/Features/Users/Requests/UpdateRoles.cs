using CryptoBank.Domain.Authorization;
using CryptoBank.WebAPI.Common.Validation;
using FluentValidation;
using MediatR;

namespace CryptoBank.WebAPI.Features.Users.Requests;

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
