namespace CryptoBank.WebAPI.Tests.Integration.Common.Errors;

public record ProblemDetailsErrorContract
{
    public string Field { get; init; }

    public string Message { get; init; }

    public string Code { get; init; }
}
