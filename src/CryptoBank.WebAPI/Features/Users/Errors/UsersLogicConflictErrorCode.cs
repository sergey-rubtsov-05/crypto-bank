namespace CryptoBank.WebAPI.Features.Users.Errors;

internal static class UsersLogicConflictErrorCode
{
    private const string Prefix = "users_logic_conflict_";

    public const string EmailAlreadyUse = Prefix + "email_already_use";
}
