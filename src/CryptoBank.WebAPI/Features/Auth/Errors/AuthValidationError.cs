using CryptoBank.WebAPI.Common.Errors;

namespace CryptoBank.WebAPI.Features.Auth.Errors;

internal class AuthValidationError : ValidationError
{
    private const string Prefix = "auth_validation_";

    public static readonly AuthValidationError RefreshTokenEmpty = new(Prefix + "refresh_token_empty");
    public static readonly AuthValidationError RefreshTokenDoesNotExist = new(Prefix + "refresh_token_does_not_exist");
    public static readonly AuthValidationError EmailIsEmpty = new(Prefix + "email_is_empty");

    private AuthValidationError(string code)
        : base(code)
    {
    }
}
