using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;
using FluentAssertions;
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
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        _cancellationToken = cancellationTokenSource.Token;

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

    private async Task WaitForCryptoDepositCreation()
    {
        while (true)
        {
            var recordWasCreated = await _database.Execute(
                dbContext => dbContext.CryptoDeposits.AnyAsync(_cancellationToken));

            if (recordWasCreated)
                break;
        }
    }

    [Fact]
    public async Task BlockWithTransferToUserDepositAddress_CryptoDepositShouldBeCreated()
    {
        var userAddress = await _database.Execute(
            async dbContext =>
            {
                var user = new User("anyEmail", "anyPasswordHash", null, _clock.UtcNow, new[] { Role.User });
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync(_cancellationToken);

                var xpub = await dbContext.Xpubs.SingleAsync(_cancellationToken);
                var masterExtPubKey = new BitcoinExtPubKey(xpub.Value, Network.RegTest).ExtPubKey;
                var derivationIndex = xpub.LastUsedDerivationIndex + 1;
                var userPubKey = masterExtPubKey.Derive(derivationIndex).PubKey;
                var userAddress = userPubKey.Hash.GetAddress(Network.RegTest);

                dbContext.DepositAddresses.Add(
                    new DepositAddress("BTC", derivationIndex, userAddress.ToString(), user.Id, xpub.Id));

                await dbContext.SaveChangesAsync(_cancellationToken);

                return userAddress;
            });

        var client = await _bitcoin.CreateClientWithWallet();

        await Mine50Btc(client);

        const int expectedAmountBtc = 28;
        await client.SendToAddressAsync(
            userAddress,
            new Money(expectedAmountBtc, MoneyUnit.BTC),
            new SendToAddressParameters { FeeRate = new FeeRate(1m) },
            _cancellationToken);

        await client.GenerateAsync(1, _cancellationToken);

        await WaitForCryptoDepositCreation();

        var actualDeposit = await _database.Execute(
            dbContext =>
                dbContext.CryptoDeposits.SingleOrDefaultAsync(
                    deposit => deposit.Address.CryptoAddress == userAddress.ToString(),
                    _cancellationToken));

        actualDeposit.Should().NotBeNull();
        actualDeposit.Amount.Should().Be(expectedAmountBtc);
        actualDeposit.Confirmations.Should().Be(0);
        actualDeposit.Status.Should().Be(DepositStatus.Created);
        actualDeposit.CurrencyCode.Should().Be("BTC");
    }
}
