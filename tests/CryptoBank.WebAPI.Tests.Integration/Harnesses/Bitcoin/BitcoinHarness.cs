using CryptoBank.WebAPI.Features.Deposits.Services;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;

public class BitcoinHarness<TProgram> : IHarness<TProgram> where TProgram : class
{
    private IContainer _bitcoind;

    private WebApplicationFactory<TProgram> _factory;

    public void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        { "Features:Deposits:BitcoinNetwork", "RegTest" },
                        { "Features:Deposits:BitcoinClient:User", "user" },
                        { "Features:Deposits:BitcoinClient:Password", "password" },
                        { "Features:Deposits:BitcoinClient:Host", _bitcoind.Hostname },
                        { "Features:Deposits:BitcoinClient:Port", _bitcoind.GetMappedPublicPort(8332).ToString() },
                    });
            });
    }

    public async Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken = default)
    {
        _factory = factory;

        _bitcoind = new ContainerBuilder().WithImage("kylemanna/bitcoind:latest")
            .WithResourceMapping(BitcoinConfig.GetBytes(), "/bitcoin/.bitcoin/bitcoin.conf")
            .WithPortBinding(8332, true)
            .Build();

        await _bitcoind.StartAsync(cancellationToken);
    }

    public async Task Stop()
    {
        await _bitcoind.StopAsync();
        await _bitcoind.DisposeAsync();
    }

    public async Task<RPCClient> CreateClient()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return scope.ServiceProvider.GetRequiredService<BitcoinClientFactory>().Create();
    }

    public async Task<RPCClient> CreateClientWithWallet()
    {
        var client = await CreateClient();
        return await client.CreateWalletAsync("AnyRegTestWallet");
    }
}

//todo: implement container builder with waiter for bitcoind
internal class BitcoindBuilder : ContainerBuilder
{
}

internal class BitcoindContainer
{
}
