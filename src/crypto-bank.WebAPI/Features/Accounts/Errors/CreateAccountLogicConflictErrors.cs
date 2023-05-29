namespace crypto_bank.WebAPI.Features.Accounts.Errors;

public static class CreateAccountLogicConflictErrors
{
    private const string Prefix = "create_account_";

    public const string MaximumNumberOfAccountsReached = Prefix + "maximum_number_of_accounts_reached";
}
