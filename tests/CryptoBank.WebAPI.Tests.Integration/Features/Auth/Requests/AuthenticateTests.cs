using System.Net;
using CryptoBank.WebAPI.Features.Auth.Requests;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using FluentAssertions;
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

    private static void VerifyApiModelValidationFailedResponse(
        RestResponse<ProblemDetailsContract> restResponse,
        string expectedField,
        string expectedCode)
    {
        restResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        restResponse.ContentType.Should().Be("application/problem+json");

        var problemDetails = restResponse.Data;

        problemDetails.Should().NotBeNull();
        problemDetails.Title.Should().Be("Api model validation failed");
        problemDetails.Detail.Should().Be("One or more validation errors have occurred");
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);

        var actualErrors = problemDetails.Errors;
        actualErrors.Should().NotBeNull();
        actualErrors.Should().ContainSingle();

        var actualError = actualErrors.Single();
        actualError.Field.Should().Be(expectedField);
        actualError.Code.Should().Be(expectedCode);
    }

    [Fact]
    public async Task RequestValidator_EmailIsEmpty_ReturnsValidationError()
    {
        var httpClient = _factory.CreateClient();
        var restClient = new RestClient(httpClient);

        var authRequest = new Authenticate.Request("", "anyPassword");

        var restResponse =
            await restClient.ExecutePostAsync<ProblemDetailsContract>(
                new RestRequest("/auth").AddJsonBody(authRequest));

        VerifyApiModelValidationFailedResponse(restResponse, "email", "auth_validation_email_is_empty");
    }
}
