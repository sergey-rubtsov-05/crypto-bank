using System.Net;
using CryptoBank.WebAPI.Features.Deposits.Options;
using Microsoft.Extensions.Options;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Features.Deposits.Services;

public class BitcoinClientFactory
{
    private readonly BitcoinClientOptions _bitcoinClientOptions;
    private readonly NetworkSource _networkSource;

    public BitcoinClientFactory(IOptions<DepositsOptions> depositsOptions, NetworkSource networkSource)
    {
        _bitcoinClientOptions = depositsOptions.Value.BitcoinClient;
        _networkSource = networkSource;
    }

    public RPCClient Create()
    {
        var credentials = new NetworkCredential(_bitcoinClientOptions.User, _bitcoinClientOptions.Password);
        var address = new UriBuilder("http", _bitcoinClientOptions.Host, _bitcoinClientOptions.Port).Uri;
        var network = _networkSource.Get();

        return new RPCClient(credentials, address, network);
    }
}
