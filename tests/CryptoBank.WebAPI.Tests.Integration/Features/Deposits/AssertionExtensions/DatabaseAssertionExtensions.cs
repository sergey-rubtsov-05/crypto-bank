using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;

public static class DatabaseAssertionExtensions
{
    public static async Task ShouldContainDeposit(
        this DatabaseHarness<Program, CryptoBankDbContext> database,
        BitcoinPubKeyAddress userAddress,
        decimal expectedAmountBtc,
        CancellationToken cancellationToken = default)
    {
        var actualDeposit = await database.Execute(
            dbContext =>
                dbContext.CryptoDeposits.SingleOrDefaultAsync(
                    deposit => deposit.Address.CryptoAddress == userAddress.ToString(),
                    cancellationToken));

        actualDeposit.Should().NotBeNull();
        actualDeposit.Amount.Should().Be(expectedAmountBtc);
        actualDeposit.Confirmations.Should().Be(0);
        actualDeposit.Status.Should().Be(DepositStatus.Created);
        actualDeposit.CurrencyCode.Should().Be("BTC");
    }
}
