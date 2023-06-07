namespace CryptoBank.WebAPI.Features.Auth.Errors;

public static class AuthLogicConflictErrorCode
{
    private const string Prefix = "auth_";

    public const string RefreshTokenEmpty = Prefix + "refresh_token_empty";
    public const string RefreshTokenDoesNotExist = Prefix + "refresh_token_does_not_exist";
    public const string RefreshTokenAlreadyRevoked = Prefix + "refresh_token_already_revoked";
    public const string RefreshTokenExpired = Prefix + "refresh_token_expired";
}
