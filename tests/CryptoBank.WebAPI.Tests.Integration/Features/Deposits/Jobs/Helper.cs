using CryptoBank.Common;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using NBitcoin.RPC;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Jobs;

internal class Helper
{
    private readonly CancellationToken _cancellationToken;
    private readonly IClock _clock;
    private readonly DatabaseHarness<Program> _database;

    public Helper(IClock clock, DatabaseHarness<Program> database, CancellationToken cancellationToken)
    {
        _clock = clock;
        _database = database;
        _cancellationToken = cancellationToken;
    }

    public async Task Mine50Btc(RPCClient client)
    {
        var bitcoinAddress = await client.GetNewAddressAsync(new GetNewAddressRequest(), _cancellationToken);

        await client.GenerateToAddressAsync(101, bitcoinAddress, _cancellationToken);
    }

    public async Task WaitForCryptoDepositCreation(int expectedCount)
    {
        while (true)
        {
            var actualCount = await _database.Execute(
                dbContext => dbContext.CryptoDeposits.CountAsync(_cancellationToken));

            if (actualCount >= expectedCount)
                break;
        }
    }

    public async Task<List<BitcoinPubKeyAddress>> CreateBitcoinAddresses(int number)
    {
        return await _database.Execute(
            async dbContext =>
            {
                var xpub = await dbContext.Xpubs.SingleAsync(_cancellationToken);
                var masterExtPubKey = new BitcoinExtPubKey(xpub.Value, Network.RegTest).ExtPubKey;

                var userAddresses = new List<BitcoinPubKeyAddress>();

                for (var i = 1; i <= number; i++)
                {
                    var user = new User(
                        $"anyEmail{i}",
                        $"anyPasswordHash{i}",
                        null,
                        _clock.UtcNow,
                        new[] { Role.User });

                    const string currencyCode = "BTC";
                    var account = new Account($"ACC{i}")
                    {
                        User = user, Currency = currencyCode, OpenedAt = _clock.UtcNow,
                    };

                    var derivationIndex = (uint)i;

                    var userPubKey = masterExtPubKey.Derive(derivationIndex).PubKey;
                    var userBitcoinAddress = userPubKey.Hash.GetAddress(Network.RegTest);
                    var depositAddress = new DepositAddress(
                        currencyCode,
                        derivationIndex,
                        userBitcoinAddress.ToString(),
                        user,
                        xpub);

                    userAddresses.Add(userBitcoinAddress);
                    await dbContext.Users.AddAsync(user, _cancellationToken);
                    await dbContext.Accounts.AddAsync(account, _cancellationToken);
                    await dbContext.DepositAddresses.AddAsync(depositAddress, _cancellationToken);
                }

                await dbContext.SaveChangesAsync(_cancellationToken);

                return userAddresses;
            });
    }
}
