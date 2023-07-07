using CryptoBank.Common;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Deposits.Requests;
using CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Common;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using CryptoBank.WebAPI.Tests.Integration.Features.Deposits.AssertionExtensions;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Authenticators;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits.Requests;

public class GetDepositAddressTests : IntegrationTestsBase
{
    private IClock _clock;
    private AuthHelper _authHelper;

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
        var request = new GetDepositAddress.Request();

        var httpClient = Factory.CreateClient();
        var restClient = new RestClient(httpClient);

        var restRequest = new RestRequest("/deposits/depositAddress").AddJsonBody(request);

        if (accessToken != null)
            restRequest.Authenticator = new JwtAuthenticator(accessToken);

        var restResponse = await restClient.ExecutePutAsync<TResponse>(restRequest);

        return restResponse;
    }

    [Fact]
    private async Task DepositAddressDoesNotExist_ReturnNewAddressInResponse()
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
