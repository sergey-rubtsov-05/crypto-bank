using CryptoBank.WebAPI.Common.Errors;

namespace CryptoBank.WebAPI.Features.Deposits.Errors;

internal class DepositsLogicConflictError : LogicConflictError
{
    private const string Prefix = "deposits_logic_conflict_";

    public static readonly DepositsLogicConflictError
        ServiceIsNotConfigured = new(Prefix + "service_is_not_configured");

    private DepositsLogicConflictError(string code)
        : base(code)
    {
    }
}
