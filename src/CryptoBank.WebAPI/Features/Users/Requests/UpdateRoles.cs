using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Validation;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;

namespace CryptoBank.WebAPI.Features.Users.Requests;

public class UpdateRoles
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

    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request>
    {
        private readonly CryptoBankDbContext _dbContext;

        public RequestHandler(CryptoBankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var updatedUser = new User(request.UserId) { Roles = request.NewRoles };

            _dbContext.Attach(updatedUser);
            _dbContext.Entry(updatedUser).Property(user => user.Roles).IsModified = true;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
