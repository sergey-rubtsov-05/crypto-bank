using System.Diagnostics;
using CryptoBank.Common;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using NBitcoin;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Jobs;

public class BlockchainDepositConfirmationScannerTests : IAsyncLifetime
{
    private readonly BitcoinHarness<Program> _bitcoin;
    private readonly Mock<IClock> _clockMock;
    private readonly DatabaseHarness<Program> _database;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TimeSpan _scanInterval = TimeSpan.FromSeconds(1);

    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private Helper _helper;
    private AsyncServiceScope _scope;

    public BlockchainDepositConfirmationScannerTests()
    {
        _bitcoin = new BitcoinHarness<Program>();
        _database = new DatabaseHarness<Program>();
        _clockMock = new Mock<IClock>();
        _clockMock
            .Setup(clock => clock.UtcNow)
            .Returns(07.October(2023).AsUtc());

        _factory = new WebApplicationFactory<Program>()
            .WithHarness(_bitcoin)
            .WithHarness(_database)
            .WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureServices(
                        services =>
                        {
                            services.AddSingleton(_clockMock.Object);
                        });

                    builder.ConfigureAppConfiguration(
                        (_, configBuilder) =>
                        {
                            configBuilder.AddInMemoryCollection(
                                new Dictionary<string, string>
                                {
                                    { "Features:Deposits:BitcoinBlockchainScanInterval", _scanInterval.ToString() },
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

        _helper = new Helper(_cancellationToken, _clockMock.Object, _database);
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _factory.DisposeAsync();

        await _database.Stop();
        await _bitcoin.Stop();
    }

    private async Task<CryptoDeposit> LoadCryptoDeposit(long cryptoDepositId)
    {
        return await _database.Execute(
            dbContext => dbContext.CryptoDeposits.SingleAsync(
                x => x.Id == cryptoDepositId,
                _cancellationToken));
    }

    private async Task<CryptoDeposit> CreateCryptoDeposit(RPCClient client)
    {
        var userAddresses = await _helper.CreateBitcoinAddresses(1);
        var userAddress = userAddresses.Single();
        const int expectedAmountBtc = 28;
        await client.SendToAddressAsync(
            userAddress,
            new Money(expectedAmountBtc, MoneyUnit.BTC),
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

        var cryptoDeposit = await CreateCryptoDeposit(client);

        var expectedConfirmationTime = cryptoDeposit.CreatedAt.UtcDateTime + TimeSpan.FromMinutes(1);
        _clockMock.Setup(clock => clock.UtcNow).Returns(expectedConfirmationTime);

        const int numberOfConfirmationBlocks = 5;
        await client.GenerateAsync(numberOfConfirmationBlocks, _cancellationToken);
        await Task.Delay(_scanInterval + TimeSpan.FromSeconds(1), _cancellationToken);

        var actualCryptoDeposit = await LoadCryptoDeposit(cryptoDeposit.Id);
        actualCryptoDeposit.ShouldBeConfirmed(numberOfConfirmationBlocks + 1, expectedConfirmationTime);
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

        var actualCryptoDeposit = await LoadCryptoDeposit(cryptoDeposit.Id);
        actualCryptoDeposit.ShouldBePending(numberOfConfirmationBlocks + 1, expectedScannedTime);
    }
}
