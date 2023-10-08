using System.Diagnostics;
using CryptoBank.Common;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;
using Microsoft.AspNetCore.Mvc.Testing;
using NBitcoin;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Jobs;

public class BlockchainDepositScannerTests : IAsyncLifetime
{
    private readonly BitcoinHarness<Program> _bitcoin;
    private readonly DatabaseHarness<Program> _database;
    private readonly WebApplicationFactory<Program> _factory;

    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private IClock _clock;
    private Helper _helper;
    private AsyncServiceScope _scope;

    public BlockchainDepositScannerTests()
    {
        _bitcoin = new BitcoinHarness<Program>();
        _database = new DatabaseHarness<Program>();

        _factory = new WebApplicationFactory<Program>()
            .WithHarness(_bitcoin)
            .WithHarness(_database)
            .WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureAppConfiguration(
                        (_, configBuilder) =>
                        {
                            configBuilder.AddInMemoryCollection(
                                new Dictionary<string, string>
                                {
                                    {
                                        "Features:Deposits:BitcoinBlockchainScanInterval",
                                        TimeSpan.FromSeconds(1).ToString()
                                    },
                                });
                        });
                });
    }

    public async Task InitializeAsync()
    {
        var delay = Debugger.IsAttached ? TimeSpan.FromMinutes(10) : TimeSpan.FromSeconds(60);
        _cancellationTokenSource = new CancellationTokenSource(delay);
        _cancellationToken = _cancellationTokenSource.Token;

        await _bitcoin.Start(_factory, _cancellationToken);
        await _database.Start(_factory, _cancellationToken);

        var _ = _factory.Server;
        _scope = _factory.Services.CreateAsyncScope();

        _clock = _scope.ServiceProvider.GetRequiredService<IClock>();
        _helper = new Helper(_cancellationToken, _clock, _database);
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _factory.DisposeAsync();

        await _database.Stop();
        await _bitcoin.Stop();
    }

    [Fact]
    public async Task BlockWithTransferToTwoUserDepositAddress_TwoCryptoDepositShouldBeCreated()
    {
        var bitcoinAddresses = await _helper.CreateBitcoinAddresses(2);

        var client = await _bitcoin.CreateClientWithWallet();

        await _helper.Mine50Btc(client);

        var expectedDeposits = new List<(BitcoinPubKeyAddress toAddress, decimal amount)>();
        decimal initAmountBtc = 5;

        foreach (var userAddress in bitcoinAddresses)
        {
            var amountBtc = initAmountBtc += 10;
            await client.SendToAddressAsync(
                userAddress,
                new Money(amountBtc, MoneyUnit.BTC),
                new SendToAddressParameters { FeeRate = new FeeRate(1m) },
                _cancellationToken);

            expectedDeposits.Add((userAddress, amountBtc));
        }

        await client.GenerateAsync(1, _cancellationToken);

        await _helper.WaitForCryptoDepositCreation(expectedDeposits.Count);

        foreach (var (expectedAddress, expectedAmount) in expectedDeposits)
        {
            await _database.ShouldContainDeposit(expectedAddress, expectedAmount, _cancellationToken);
        }
    }

    [Fact]
    public async Task BlockWithTransferToUserDepositAddress_CryptoDepositShouldBeCreated()
    {
        var userAddresses = await _helper.CreateBitcoinAddresses(1);

        var client = await _bitcoin.CreateClientWithWallet();

        await _helper.Mine50Btc(client);

        var userAddress = userAddresses.Single();
        const int expectedAmountBtc = 28;
        await client.SendToAddressAsync(
            userAddress,
            new Money(expectedAmountBtc, MoneyUnit.BTC),
            new SendToAddressParameters { FeeRate = new FeeRate(1m) },
            _cancellationToken);

        await client.GenerateAsync(1, _cancellationToken);

        await _helper.WaitForCryptoDepositCreation(1);

        await _database.ShouldContainDeposit(userAddress, expectedAmountBtc, _cancellationToken);
    }
}
