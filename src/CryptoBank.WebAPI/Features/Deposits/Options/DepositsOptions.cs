namespace CryptoBank.WebAPI.Features.Deposits.Options;

public class DepositsOptions
{
    public BitcoinNetwork BitcoinNetwork { get; init; }
}

public enum BitcoinNetwork
{
    Main = 1,
    Test = 2,
}
