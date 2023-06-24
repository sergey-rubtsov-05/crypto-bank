using CryptoBank.WebAPI.Common.Errors;

namespace CryptoBank.WebAPI.Features.Users.Errors;

internal class UsersLogicConflictError : LogicConflictError
{
    private const string Prefix = "users_logic_conflict_";

    public static readonly UsersLogicConflictError EmailAlreadyUse = new(Prefix + "email_already_use");

    private UsersLogicConflictError(string code) : base(code)
    {
    }
}
