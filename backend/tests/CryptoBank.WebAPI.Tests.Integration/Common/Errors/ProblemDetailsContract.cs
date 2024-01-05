namespace CryptoBank.WebAPI.Tests.Integration.Common.Errors;

public record ProblemDetailsContract
{
    public string Title { get; init; }

    public string Type { get; init; }

    public string Detail { get; init; }

    public int Status { get; init; }

    public string TraceId { get; init; }

    public ProblemDetailsErrorContract[] Errors { get; init; }
}
