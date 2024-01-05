using CryptoBank.WebAPI.Common.Errors;

namespace CryptoBank.WebAPI.Features.Deposits.Errors;

internal class DepositsLogicConflictError : LogicConflictError
{
    private const string Prefix = "deposits_logic_conflict_";

    public static readonly DepositsLogicConflictError
        ServiceIsNotConfigured = new(Prefix + "service_is_not_configured");

    public static readonly DepositsLogicConflictError
        CouldNotFindAccountForDeposit = new(Prefix + "could_not_find_account_for_deposit");

    private DepositsLogicConflictError(string code)
        : base(code)
    {
    }
}
