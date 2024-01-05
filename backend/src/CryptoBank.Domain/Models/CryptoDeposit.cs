namespace CryptoBank.Domain.Models;

public record CryptoDeposit
{
    public CryptoDeposit(
        int userId,
        int addressId,
        decimal amount,
        string currencyCode,
        DateTimeOffset createdAt,
        string txId)
    {
        UserId = userId;
        AddressId = addressId;
        Amount = amount;
        CurrencyCode = currencyCode;
        CreatedAt = createdAt;
        TxId = txId;
        Confirmations = 0;
        Status = DepositStatus.Created;
    }

    public long Id { get; init; }

    public int UserId { get; init; }

    public int AddressId { get; init; }

    public DepositAddress Address { get; init; } = null!;

    public decimal Amount { get; init; }

    public string CurrencyCode { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public string TxId { get; init; }

    public uint Confirmations { get; init; }

    public DepositStatus Status { get; init; }

    public DateTimeOffset? ScannedAt { get; init; }

    public DateTimeOffset? ConfirmedAt { get; init; }
}

public enum DepositStatus
{
    Created = 10,
    Pending = 20,
    Confirmed = 30,
}
