using CryptoBank.WebAPI.Features.Deposits.Options;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace CryptoBank.WebAPI.Features.Deposits.Services;

public class NetworkSource
{
    private readonly DepositsOptions _depositsOptions;

    public NetworkSource(IOptions<DepositsOptions> depositsOptions)
    {
        _depositsOptions = depositsOptions.Value;
    }

    public Network Get()
    {
        return _depositsOptions.BitcoinNetwork switch
        {
            BitcoinNetwork.Main => Network.Main,
            BitcoinNetwork.Test => Network.TestNet,
            BitcoinNetwork.RegTest => Network.RegTest,
            _ => throw new ArgumentOutOfRangeException(nameof(_depositsOptions.BitcoinNetwork)),
        };
    }
}
