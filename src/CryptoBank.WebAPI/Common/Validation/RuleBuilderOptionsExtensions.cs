using CryptoBank.WebAPI.Common.Errors;
using FluentValidation;

namespace CryptoBank.WebAPI.Common.Validation;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        ValidationError error)
    {
        return rule.WithErrorCode(error.Code);
    }
}
