using System.Net;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Deposits.Requests;
using CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using RestSharp;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;

public static class GetDepositAddressResponseExtensions
{
    public static async Task ShouldBeValidGetDepositAddressResponse(
        this RestResponse<GetDepositAddress.Response> restResponse,
        User user,
        CryptoBankDbContext dbContext)
    {
        restResponse.StatusCode.Should().Be(HttpStatusCode.OK, restResponse.Content);
        restResponse.ContentType.Should().Be("application/json");

        var response = restResponse.Data;
        response.ShouldNotBeNull();

        var xpub = await dbContext.Xpubs.SingleAsync();
        var expectedCryptoAddress = CreateCryptoAddress(xpub);

        response.ShouldContainValidCryptoAddress(expectedCryptoAddress);
        await dbContext.DepositAddresses.ShouldContainValidCryptoAddress(expectedCryptoAddress, user, xpub);
    }

    private static string CreateCryptoAddress(Xpub xpub)
    {
        var masterExtPubKey = new BitcoinExtPubKey(xpub.Value, Network.TestNet).ExtPubKey;
        var userPubKey = masterExtPubKey.Derive(xpub.LastUsedDerivationIndex).PubKey;
        var cryptoAddress = userPubKey.Hash.GetAddress(Network.TestNet).ToString();
        return cryptoAddress;
    }

    private static void ShouldContainValidCryptoAddress(
        this GetDepositAddress.Response response,
        string expectedCryptoAddress)
    {
        var cryptoAddress = response.CryptoAddress;

        cryptoAddress.Should().NotBeNullOrEmpty();
        cryptoAddress.Should().Be(expectedCryptoAddress);
    }

    private static async Task ShouldContainValidCryptoAddress(
        this DbSet<DepositAddress> depositAddresses,
        string expectedCryptoAddress,
        User user,
        Xpub xpub)
    {
        var depositAddress = await depositAddresses.SingleOrDefaultAsync(address => address.UserId == user.Id);

        depositAddress.ShouldNotBeNull("deposit address should be created");
        depositAddress.XpubId.Should().Be(xpub.Id);
        depositAddress.CryptoAddress.Should().Be(expectedCryptoAddress);
    }
}
