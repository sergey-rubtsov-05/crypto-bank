namespace crypto_bank.WebAPI.Features.Users.Errors;

internal static class RegisterUserLogicConflictError
{
    private const string Prefix = "register_user_";

    public const string AlreadyExists = Prefix + "already_exists";
}
