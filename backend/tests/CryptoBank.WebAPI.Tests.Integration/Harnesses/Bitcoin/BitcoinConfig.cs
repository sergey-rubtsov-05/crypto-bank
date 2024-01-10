namespace CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;

public static class BitcoinConfig
{
    public static byte[] GetBytes()
    {
        return @"
# Generated by https://jlopp.github.io/bitcoin-core-config-generator/

# [rpc]
# Accept command line and JSON-RPC commands.
server=1

# [core]
# Maintain a full transaction index, used by the getrawtransaction rpc call.
txindex=1

# [chain]
# Regression Test Network
chain=regtest

# [debug]
# Enable debug logging for all categories.
debug=1
# Log IP Addresses in debug output.
logips=1
# Log timestamps with microsecond precision.
logtimemicros=1

rpcuser=user
rpcpassword=password

# [Sections]
# Most options automatically apply to mainnet, testnet, and regtest networks.
# If you want to confine an option to just one network, you should add it in the relevant section.
# EXCEPTIONS: The options addnode, connect, port, bind, rpcport, rpcbind and wallet
# only apply to mainnet unless they appear in the appropriate section below.

# Options only for regtest
[regtest]

# [network]
# Bind to given address and always listen on it. (default: 0.0.0.0). Use [host]:port notation for IPv6. Append =onion to tag any incoming connections to that address and port as incoming Tor connections
rpcbind=0.0.0.0
# Allow JSON-RPC connections from specified source. Valid for <ip> are a single IP (e.g. 1.2.3.4), a network/netmask (e.g. 1.2.3.4/255.255.255.0) or a network/CIDR (e.g. 1.2.3.4/24). This option can be specified multiple times.
rpcallowip=0.0.0.0/0
"u8.ToArray();
    }
}