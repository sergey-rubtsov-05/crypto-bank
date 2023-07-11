namespace CryptoBank.Domain.Models;

public record DepositAddress
{
    public DepositAddress(string currencyCode, uint derivationIndex, string cryptoAddress, int userId, int xpubId)
    {
        CurrencyCode = currencyCode;
        DerivationIndex = derivationIndex;
        CryptoAddress = cryptoAddress;
        UserId = userId;
        XpubId = xpubId;
    }

    public int Id { get; init; }
    public string CurrencyCode { get; init; }
    public int UserId { get; init; }
    public User User { get; init; } = null!;
    public int XpubId { get; init; }
    public Xpub Xpub { get; init; } = null!;
    public uint DerivationIndex { get; init; }
    public string CryptoAddress { get; init; }
}
