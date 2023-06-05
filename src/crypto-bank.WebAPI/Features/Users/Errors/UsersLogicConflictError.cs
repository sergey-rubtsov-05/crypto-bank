namespace crypto_bank.WebAPI.Features.Users.Errors;

internal static class UsersLogicConflictError
{
    private const string Prefix = "users_";

    public const string EmailAlreadyUse = Prefix + "email_already_use";
    public const string UserNotFound = Prefix + "user_not_found";
}
