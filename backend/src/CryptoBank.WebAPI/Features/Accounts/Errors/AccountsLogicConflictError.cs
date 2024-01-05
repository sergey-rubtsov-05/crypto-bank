using CryptoBank.WebAPI.Common.Errors;

namespace CryptoBank.WebAPI.Features.Accounts.Errors;

public class AccountsLogicConflictError : LogicConflictError
{
    private const string Prefix = "accounts_";

    public static readonly AccountsLogicConflictError MaximumNumberOfAccountsReached =
        new(Prefix + "maximum_number_of_accounts_reached");

    private AccountsLogicConflictError(string code) : base(code)
    {
    }
}
