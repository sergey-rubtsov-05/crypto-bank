namespace CryptoBank.WebAPI.Features.Auth.Errors;

public static class AuthLogicConflictErrorCode
{
    private const string Prefix = "auth_logic_conflict_";

    public const string RefreshTokenAlreadyRevoked = Prefix + "refresh_token_already_revoked";
    public const string RefreshTokenExpired = Prefix + "refresh_token_expired";
}
