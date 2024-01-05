namespace CryptoBank.WebAPI.Features.Deposits.Options;

public record DepositsOptions
{
    public TimeSpan BitcoinBlockchainScanInterval { get; init; }
    public BitcoinNetwork BitcoinNetwork { get; init; }
    public BitcoinClientOptions BitcoinClient { get; init; } = null!;
    public int BitcoinTxConfirmationCount { get; init; }
}

public record BitcoinClientOptions
{
    public string User { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string Host { get; init; } = null!;
    public int Port { get; init; }
}

public enum BitcoinNetwork
{
    Main = 1,
    Test = 2,
    RegTest = 3,
}
