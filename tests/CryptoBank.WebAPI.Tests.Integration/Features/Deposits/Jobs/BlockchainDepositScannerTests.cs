using CryptoBank.Common;
using CryptoBank.WebAPI.Tests.Integration.Common.Factories;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;
using Microsoft.AspNetCore.Mvc.Testing;
using NBitcoin;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Jobs;

[Collection(DepositsTestsCollection.Name)]
public class BlockchainDepositScannerTests : IAsyncLifetime
{
    private readonly BitcoinHarness<Program> _bitcoin;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _cancellationTokenSource = Factory.CreateCancellationTokenSource(60);
    private readonly DatabaseHarness<Program> _database;
    private readonly WebApplicationFactory<Program> _factory;

    private IClock _clock;
    private Helper _helper;
    private AsyncServiceScope _scope;

    public BlockchainDepositScannerTests(DepositsTestFixture testFixture)
    {
        _bitcoin = testFixture.Bitcoin;
        _database = testFixture.Database;

        _factory = testFixture.Factory;
        _cancellationToken = _cancellationTokenSource.Token;
    }

    public async Task InitializeAsync()
    {
        await _database.Clear(_cancellationToken);

        _scope = _factory.Services.CreateAsyncScope();

        _clock = _scope.ServiceProvider.GetRequiredService<IClock>();
        _helper = new Helper(_clock, _database, _cancellationToken);
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }

    [Fact]
    public async Task BlockWithTransferToTwoUserDepositAddress_TwoCryptoDepositShouldBeCreated()
    {
        var bitcoinAddresses = await _helper.CreateBitcoinAddresses(2);

        var client = await _bitcoin.CreateClientWithWallet();

        await _helper.Mine50Btc(client);

        var expectedDeposits = new List<(BitcoinPubKeyAddress toAddress, decimal amount)>();
        decimal initAmountBtc = 1;

        foreach (var userAddress in bitcoinAddresses)
        {
            var amountBtc = initAmountBtc += 2;
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
        const int expectedAmountBtc = 3;
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
