namespace CryptoBank.Domain.Models;

public record BitcoinBlockchainStatus(int LastProcessedBlockHeight)
{
    public int Id { get; init; }
}
