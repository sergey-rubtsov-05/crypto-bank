using CryptoBank.Common;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Deposits.Requests;
using CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Common;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using CryptoBank.WebAPI.Tests.Integration.Common.Factories;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Requests;

[Collection(DepositsTestsCollection.Name)]
public class GetDepositAddressTests : IAsyncLifetime
{
    private readonly IClock _clock;
    private readonly CancellationTokenSource _cts = Factory.CreateCancellationTokenSource(60);
    private readonly DatabaseHarness<Program> _database;
    private readonly DepositsTestFixture _fixture;
    private readonly HttpClientHarness<Program> _httpClient;

    private AuthHelper _authHelper;
    private AsyncServiceScope _scope;


    public GetDepositAddressTests(DepositsTestFixture testFixture)
    {
        _fixture = testFixture;
        _database = _fixture.Database;
        _httpClient = _fixture.HttpClient;
        _clock = _fixture.ClockMock.Object;
        _fixture.ClockMock
            .Setup(clock => clock.UtcNow)
            .Returns(() => DateTime.UtcNow);
    }

    public async Task InitializeAsync()
    {
        await _database.Clear(_cts.Token);

        _scope = _fixture.Factory.Services.CreateAsyncScope();
        var authOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>();
        _authHelper = new AuthHelper(_clock, authOptions);
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }

    private async Task<RestResponse<TResponse>> ExecuteRequest<TResponse>(string accessToken = null)
    {
        var httpClient = _httpClient.CreateClient();
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
        var users = Enumerable.Range(1, 10)
            .Select(i => new User($"anyEmail{i}", "anyPasswordHash", null, _clock.UtcNow, new[] { Role.User }))
            .ToArray();

        await _database.Save(users);

        var requestTasks = users
            .Select(
                user =>
                {
                    var accessToken = _authHelper.CreateAccessToken(user.Id, user.Roles);
                    return ExecuteRequest<GetDepositAddress.Response>(accessToken);
                })
            .ToList();

        var responses = await Task.WhenAll(requestTasks);
        responses.Should().AllSatisfy(response => response.ShouldBeValidJsonResponse());

        var depositAddresses = await _database.Execute(dbContext => dbContext.DepositAddresses.ToListAsync());
        var previousDerivationIndex = depositAddresses.Select(address => address.DerivationIndex).Min() - 1;
        foreach (var depositAddress in depositAddresses.OrderBy(address => address.DerivationIndex))
        {
            depositAddress.DerivationIndex.Should().Be(previousDerivationIndex + 1);
            previousDerivationIndex = depositAddress.DerivationIndex;
        }
    }

    [Fact]
    private async Task DepositAddress_SecondCallReturnsTheSameAddress()
    {
        var user = new User("anyEmail", "anyPasswordHash", null, _clock.UtcNow, new[] { Role.User });
        await _database.Save(user);

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
        await _database.Save(user);

        var accessToken = _authHelper.CreateAccessToken(user.Id, user.Roles);

        var restResponse = await ExecuteRequest<GetDepositAddress.Response>(accessToken);

        await _database.Execute(
            async dbContext =>
            {
                await restResponse.ShouldBeValidGetDepositAddressResponse(user, dbContext);
            });
    }

    [Fact]
    private async Task UnauthorizedRequest_ReturnsUnauthorizedResponse()
    {
        var restResponse = await ExecuteRequest<ProblemDetailsContract>();

        restResponse.ShouldBeUnauthorizedResponse();
    }
}
