using CryptoBank.WebAPI.Common.Errors;

namespace CryptoBank.WebAPI.Features.Auth.Errors;

public class AuthLogicConflictError : LogicConflictError
{
    private const string Prefix = "auth_logic_conflict_";

    public static readonly AuthLogicConflictError RefreshTokenAlreadyRevoked = new(Prefix + "refresh_token_already_revoked");
    public static readonly AuthLogicConflictError RefreshTokenExpired = new(Prefix + "refresh_token_expired");

    private AuthLogicConflictError(string code) : base(code)
    {
    }
}
