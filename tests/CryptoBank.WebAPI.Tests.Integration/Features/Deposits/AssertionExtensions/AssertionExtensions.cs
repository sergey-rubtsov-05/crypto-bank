using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;

public static class AssertionExtensions
{
    public static async Task ShouldContainDeposit(
        this DatabaseHarness<Program> database,
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

    public static void ShouldBePending(
        this CryptoDeposit actualDeposit,
        uint expectedConfirmations,
        DateTimeOffset expectedScannedTime)
    {
        actualDeposit.Should().NotBeNull();
        actualDeposit.Confirmations.Should().Be(expectedConfirmations);
        actualDeposit.Status.Should().Be(DepositStatus.Pending);
        actualDeposit.ScannedAt.Should().Be(expectedScannedTime);
    }

    public static void ShouldBeConfirmed(
        this CryptoDeposit actualDeposit,
        uint expectedConfirmations,
        DateTimeOffset expectedConfirmedAt)
    {
        actualDeposit.Should().NotBeNull();
        actualDeposit.Confirmations.Should().Be(expectedConfirmations);
        actualDeposit.Status.Should().Be(DepositStatus.Confirmed);
        actualDeposit.ConfirmedAt.Should().Be(expectedConfirmedAt);
    }
}
