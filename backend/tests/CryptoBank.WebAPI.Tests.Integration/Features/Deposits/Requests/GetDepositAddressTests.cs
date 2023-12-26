using CryptoBank.Common;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Deposits.Requests;
using CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Common;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Authenticators;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Requests;

public class GetDepositAddressTests : IntegrationTestsBase
{
    private AuthHelper _authHelper;
    private IClock _clock;

    protected override void ConfigureService(IServiceCollection services)
    {
        services.AddSingleton<AuthHelper>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _clock = Scope.ServiceProvider.GetRequiredService<IClock>();
        _authHelper = Scope.ServiceProvider.GetRequiredService<AuthHelper>();
    }

    public override async Task DisposeAsync()
    {
        await DbContext.Users.ExecuteDeleteAsync();
        await DbContext.Xpubs.ExecuteDeleteAsync();

        await base.DisposeAsync();
    }

    private async Task<RestResponse<TResponse>> ExecuteRequest<TResponse>(string accessToken = null)
    {
        var httpClient = Factory.CreateClient();
        var restClient = new RestClient(httpClient);

        var restRequest = new RestRequest("/deposits/depositAddress");

        if (accessToken != null)
            restRequest.Authenticator = new JwtAuthenticator(accessToken);

        var restResponse = await restClient.ExecutePutAsync<TResponse>(restRequest);

        return restResponse;
    }

    [Fact]
    private async Task DepositAddress_Create10AddressesInParallel()
    {
        for (var i = 1; i <= 10; i++)
        {
            var user = new User($"anyEmail{i}", "anyPasswordHash", null, _clock.UtcNow, new[] { Role.User });
            await DbContext.Users.AddAsync(user);
        }

        await DbContext.SaveChangesAsync();

        var requestTasks = DbContext.Users.ToList()
            .Select(
                user =>
                {
                    var accessToken = _authHelper.CreateAccessToken(user.Id, user.Roles);
                    return ExecuteRequest<GetDepositAddress.Response>(accessToken);
                })
            .ToList();

        var responses = await Task.WhenAll(requestTasks);
        responses.Should().AllSatisfy(response => response.ShouldBeValidJsonResponse());

        uint previousDerivationIndex = 0;
        foreach (var depositAddress in DbContext.DepositAddresses.OrderBy(address => address.DerivationIndex))
        {
            depositAddress.DerivationIndex.Should().Be(previousDerivationIndex + 1);
            previousDerivationIndex = depositAddress.DerivationIndex;
        }
    }

    [Fact]
    private async Task DepositAddress_SecondCallReturnsTheSameAddress()
    {
        var user = new User("anyEmail", "anyPasswordHash", null, _clock.UtcNow, new[] { Role.User });
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var accessToken = _authHelper.CreateAccessToken(user.Id, user.Roles);

        var restResponse1 = await ExecuteRequest<GetDepositAddress.Response>(accessToken);
        restResponse1.ShouldBeValidJsonResponse();

        var restResponse2 = await ExecuteRequest<GetDepositAddress.Response>(accessToken);
        restResponse2.ShouldBeValidJsonResponse();

        restResponse1.Data.CryptoAddress.Should().Be(restResponse2.Data.CryptoAddress);
    }

    [Fact]
    private async Task DepositAddressDoesNotExist_ReturnsNewAddressInResponse()
    {
        var user = new User("anyEmail", "anyPasswordHash", null, _clock.UtcNow, new[] { Role.User });
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var accessToken = _authHelper.CreateAccessToken(user.Id, user.Roles);

        var restResponse = await ExecuteRequest<GetDepositAddress.Response>(accessToken);

        await restResponse.ShouldBeValidGetDepositAddressResponse(user, DbContext);
    }

    [Fact]
    private async Task UnauthorizedRequest_ReturnsUnauthorizedResponse()
    {
        var restResponse = await ExecuteRequest<ProblemDetailsContract>();

        restResponse.ShouldBeUnauthorizedResponse();
    }
}
