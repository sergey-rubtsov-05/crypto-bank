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
        CancellationToken cancellationToken)
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

    public static async Task DepositShouldBePending(
        this DatabaseHarness<Program> database,
        long cryptoDepositId,
        uint expectedConfirmations,
        DateTimeOffset expectedScannedTime,
        CancellationToken cancellationToken)
    {
        var actualDeposit = await database.Execute(
            dbContext => dbContext.CryptoDeposits.SingleAsync(
                x => x.Id == cryptoDepositId,
                cancellationToken));

        actualDeposit.Should().NotBeNull();
        actualDeposit.Confirmations.Should().Be(expectedConfirmations);
        actualDeposit.Status.Should().Be(DepositStatus.Pending);
        actualDeposit.ScannedAt.Should().Be(expectedScannedTime);
    }

    public static async Task DepositShouldBeConfirmed(
        this DatabaseHarness<Program> database,
        long cryptoDepositId,
        uint expectedConfirmations,
        DateTimeOffset expectedConfirmedAt,
        CancellationToken cancellationToken)
    {
        var actualDeposit = await database.Execute(
            dbContext => dbContext.CryptoDeposits.SingleAsync(
                x => x.Id == cryptoDepositId,
                cancellationToken));

        actualDeposit.Should().NotBeNull();
        actualDeposit.Confirmations.Should().Be(expectedConfirmations);
        actualDeposit.Status.Should().Be(DepositStatus.Confirmed);
        actualDeposit.ConfirmedAt.Should().Be(expectedConfirmedAt);
    }

    public static async Task AccountShouldBeUpdated(
        this DatabaseHarness<Program> database,
        int userId,
        decimal expectedAmount,
        CancellationToken cancellationToken)
    {
        var actualAccount = await database.Execute(
            dbContext => dbContext.Accounts.SingleAsync(
                x => x.UserId == userId,
                cancellationToken));

        actualAccount.Should().NotBeNull();
        actualAccount.Amount.Should().Be(expectedAmount);
    }
}
