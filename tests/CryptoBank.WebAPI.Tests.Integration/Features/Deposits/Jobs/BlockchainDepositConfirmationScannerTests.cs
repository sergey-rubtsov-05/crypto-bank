using CryptoBank.Common;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Tests.Integration.Common.Factories;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;
using Microsoft.EntityFrameworkCore;
using Moq;
using NBitcoin;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Jobs;

[Collection(DepositsTestsCollection.Name)]
public class BlockchainDepositConfirmationScannerTests : IAsyncLifetime
{
    private const int AnyAmountBtc = 28;

    private readonly BitcoinHarness<Program> _bitcoin;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _cancellationTokenSource = Factory.CreateCancellationTokenSource(60);
    private readonly Mock<IClock> _clockMock;
    private readonly DatabaseHarness<Program> _database;
    private readonly Helper _helper;
    private readonly TimeSpan _scanInterval;

    public BlockchainDepositConfirmationScannerTests(DepositsTestFixture testFixture)
    {
        _bitcoin = testFixture.Bitcoin;
        _database = testFixture.Database;
        _clockMock = testFixture.ClockMock;
        _scanInterval = testFixture.ScanInterval;

        _cancellationToken = _cancellationTokenSource.Token;

        _helper = new Helper(_clockMock.Object, _database, _cancellationToken);
    }

    public async Task InitializeAsync()
    {
        await _database.Clear(_cancellationToken);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task<CryptoDeposit> CreateCryptoDeposit(RPCClient client, int amountBtc = AnyAmountBtc)
    {
        var userAddresses = await _helper.CreateBitcoinAddresses(1);
        var userAddress = userAddresses.Single();
        await client.SendToAddressAsync(
            userAddress,
            new Money(amountBtc, MoneyUnit.BTC),
            new SendToAddressParameters { FeeRate = new FeeRate(1m) },
            _cancellationToken);

        await client.GenerateAsync(1, _cancellationToken);

        await _helper.WaitForCryptoDepositCreation(1);

        return await _database.Execute(
            dbContext => dbContext.CryptoDeposits.SingleOrDefaultAsync(
                deposit => deposit.Address.CryptoAddress == userAddress.ToString(),
                _cancellationToken));
    }

    [Fact]
    public async Task CryptoDepositConfirmsWithFiveBlocks_CryptoDepositShouldBeConfirmed()
    {
        var client = await _bitcoin.CreateClientWithWallet();

        await _helper.Mine50Btc(client);

        const int expectedAmount = 15;
        var cryptoDeposit = await CreateCryptoDeposit(client, expectedAmount);

        var expectedConfirmationTime = cryptoDeposit.CreatedAt.UtcDateTime + TimeSpan.FromMinutes(1);
        _clockMock.Setup(clock => clock.UtcNow).Returns(expectedConfirmationTime);

        const int numberOfConfirmationBlocks = 5;
        await client.GenerateAsync(numberOfConfirmationBlocks, _cancellationToken);
        await Task.Delay(_scanInterval + TimeSpan.FromSeconds(1), _cancellationToken);

        await _database.DepositShouldBeConfirmed(
            cryptoDeposit.Id,
            numberOfConfirmationBlocks + 1,
            expectedConfirmationTime,
            _cancellationToken);

        await _database.AccountShouldBeUpdated(cryptoDeposit.UserId, expectedAmount, _cancellationToken);
    }

    [Fact]
    public async Task CryptoDepositConfirmsWithTwoBlocks_CryptoDepositShouldBePending()
    {
        var client = await _bitcoin.CreateClientWithWallet();

        await _helper.Mine50Btc(client);

        var cryptoDeposit = await CreateCryptoDeposit(client);

        var expectedScannedTime = cryptoDeposit.CreatedAt.UtcDateTime + TimeSpan.FromMinutes(1);
        _clockMock.Setup(clock => clock.UtcNow).Returns(expectedScannedTime);

        const int numberOfConfirmationBlocks = 2;
        await client.GenerateAsync(numberOfConfirmationBlocks, _cancellationToken);
        await Task.Delay(_scanInterval + TimeSpan.FromSeconds(1), _cancellationToken);

        await _database.DepositShouldBePending(
            cryptoDeposit.Id,
            numberOfConfirmationBlocks + 1,
            expectedScannedTime,
            _cancellationToken);
    }
}
