using System.Diagnostics;
using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Jobs;

public class BlockchainDepositScannerTests : IAsyncLifetime
{
    private readonly BitcoinHarness<Program> _bitcoin;
    private readonly DatabaseHarness<Program, CryptoBankDbContext> _database;
    private readonly WebApplicationFactory<Program> _factory;

    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private IClock _clock;
    private AsyncServiceScope _scope;

    public BlockchainDepositScannerTests()
    {
        _bitcoin = new BitcoinHarness<Program>();
        _database = new DatabaseHarness<Program, CryptoBankDbContext>();

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
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _factory.DisposeAsync();

        await _database.Stop();
        await _bitcoin.Stop();
    }

    private static async Task Mine50Btc(RPCClient client)
    {
        var bitcoinAddress = await client.GetNewAddressAsync(new GetNewAddressRequest());

        await client.GenerateToAddressAsync(101, bitcoinAddress);
    }

    private async Task WaitForCryptoDepositCreation(int expectedCount)
    {
        while (true)
        {
            var actualCount = await _database.Execute(
                dbContext => dbContext.CryptoDeposits.CountAsync(_cancellationToken));

            if (actualCount >= expectedCount)
                break;
        }
    }

    private async Task<List<BitcoinPubKeyAddress>> CreateBitcoinAddresses(CryptoBankDbContext dbContext, int number)
    {
        var xpub = await dbContext.Xpubs.SingleAsync(_cancellationToken);
        var masterExtPubKey = new BitcoinExtPubKey(xpub.Value, Network.RegTest).ExtPubKey;

        var userAddresses = new List<BitcoinPubKeyAddress>();

        for (var i = 1; i <= number; i++)
        {
            var user = new User($"anyEmail{i}", $"anyPasswordHash{i}", null, _clock.UtcNow, new[] { Role.User });
            var derivationIndex = (uint)i;

            var userPubKey = masterExtPubKey.Derive(derivationIndex).PubKey;
            var userBitcoinAddress = userPubKey.Hash.GetAddress(Network.RegTest);
            var depositAddress = new DepositAddress("BTC", derivationIndex, userBitcoinAddress.ToString(), user, xpub);

            userAddresses.Add(userBitcoinAddress);
            await dbContext.Users.AddAsync(user, _cancellationToken);
            await dbContext.DepositAddresses.AddAsync(depositAddress, _cancellationToken);
        }

        await dbContext.SaveChangesAsync(_cancellationToken);

        return userAddresses;
    }

    [Fact]
    public async Task BlockWithTransferToTwoUserDepositAddress_TwoCryptoDepositShouldBeCreated()
    {
        var bitcoinAddresses = await _database.Execute(
            async dbContext => await CreateBitcoinAddresses(dbContext, 2));

        var client = await _bitcoin.CreateClientWithWallet();

        await Mine50Btc(client);

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

        await WaitForCryptoDepositCreation(expectedDeposits.Count);

        foreach (var (expectedAddress, expectedAmount) in expectedDeposits)
        {
            await _database.ShouldContainDeposit(expectedAddress, expectedAmount, _cancellationToken);
        }
    }

    [Fact]
    public async Task BlockWithTransferToUserDepositAddress_CryptoDepositShouldBeCreated()
    {
        var userAddresses = await _database.Execute(
            dbContext => CreateBitcoinAddresses(dbContext, 1));

        var client = await _bitcoin.CreateClientWithWallet();

        await Mine50Btc(client);

        var userAddress = userAddresses.Single();
        const int expectedAmountBtc = 28;
        await client.SendToAddressAsync(
            userAddress,
            new Money(expectedAmountBtc, MoneyUnit.BTC),
            new SendToAddressParameters { FeeRate = new FeeRate(1m) },
            _cancellationToken);

        await client.GenerateAsync(1, _cancellationToken);

        await WaitForCryptoDepositCreation(1);

        await _database.ShouldContainDeposit(userAddress, expectedAmountBtc, _cancellationToken);
    }
}
