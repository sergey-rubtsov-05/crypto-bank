using CryptoBank.WebAPI.Features.Auth.Requests;
using CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using Microsoft.AspNetCore.Mvc.Testing;
using RestSharp;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Auth.Requests;

public class AuthenticateTests : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticateTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder => builder.ConfigureAppConfiguration(
                    (_, configBuilder) =>
                    {
                        configBuilder.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                {
                                    "ConnectionStrings:CryptoBankDb",
                                    "Host=localhost;Database=crypto_bank_db.tests;Username=integration_tests;Password=12345678;Maximum Pool Size=10;Connection Idle Lifetime=60;"
                                },
                            });
                    }));
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    private async Task<RestResponse<ProblemDetailsContract>> ExecuteAuthenticateRequest(string email, string password)
    {
        var authRequest = new Authenticate.Request(email, password);

        var httpClient = _factory.CreateClient();
        var restClient = new RestClient(httpClient);

        var restResponse =
            await restClient.ExecutePostAsync<ProblemDetailsContract>(
                new RestRequest("/auth").AddJsonBody(authRequest));

        return restResponse;
    }

    [Fact]
    public async Task RequestValidator_EmailIsEmpty_ReturnsValidationError()
    {
        const string email = "";
        const string password = "anyPassword";

        var restResponse = await ExecuteAuthenticateRequest(email, password);

        restResponse.ShouldBeApiModelValidationFailedResponse("email", "auth_validation_email_is_empty");
    }

    [Fact]
    private async Task RequestValidator_PasswordIsEmpty_ReturnsValidationError()
    {
        const string email = "anyEmail";
        const string password = "";

        var restResponse = await ExecuteAuthenticateRequest(email, password);

        restResponse.ShouldBeApiModelValidationFailedResponse("password", "auth_validation_password_is_empty");
    }
}
