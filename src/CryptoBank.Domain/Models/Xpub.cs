namespace CryptoBank.Domain.Models;

public record Xpub
{
    public Xpub(string currencyCode, string value)
    {
        CurrencyCode = currencyCode;
        Value = value;
    }

    public int Id { get; init; }
    public string CurrencyCode { get; init; }
    public string Value { get; init; }
    public uint LastUsedDerivationIndex { get; init; }
}
