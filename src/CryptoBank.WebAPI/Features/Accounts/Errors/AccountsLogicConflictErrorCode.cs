namespace CryptoBank.WebAPI.Features.Accounts.Errors;

public static class AccountsLogicConflictErrorCode
{
    private const string Prefix = "accounts_";

    public const string MaximumNumberOfAccountsReached = Prefix + "maximum_number_of_accounts_reached";
}
