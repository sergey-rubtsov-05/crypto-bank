namespace crypto_bank.WebAPI.Features.Accounts.Errors;

public static class AccountsLogicConflictErrors
{
    private const string Prefix = "accounts_";

    public const string MaximumNumberOfAccountsReached = Prefix + "maximum_number_of_accounts_reached";
}
