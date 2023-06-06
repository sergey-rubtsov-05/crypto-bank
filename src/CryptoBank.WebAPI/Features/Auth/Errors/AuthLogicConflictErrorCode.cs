namespace CryptoBank.WebAPI.Features.Auth.Errors;

public static class AuthLogicConflictErrorCode
{
    private const string Prefix = "auth_";

    public const string RefreshTokenEmpty = "refresh_token_empty";
}
