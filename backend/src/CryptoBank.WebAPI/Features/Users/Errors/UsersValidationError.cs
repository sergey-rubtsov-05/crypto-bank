using CryptoBank.WebAPI.Common.Errors;

namespace CryptoBank.WebAPI.Features.Users.Errors;

internal class UsersValidationError : ValidationError
{
    private const string Prefix = "users_validation_";

    public static readonly UsersValidationError UserNotFound = new(Prefix + "user_not_found");

    private UsersValidationError(string code)
        : base(code)
    {
    }
}
