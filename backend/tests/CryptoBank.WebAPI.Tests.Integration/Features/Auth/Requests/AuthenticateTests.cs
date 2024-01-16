using CryptoBank.Common;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Auth.Requests;
using CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using CryptoBank.WebAPI.Tests.Integration.Common.Factories;
using CryptoBank.WebAPI.Tests.Integration.Features.Auth.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using Microsoft.Extensions.Options;
using Moq;
using RestSharp;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Auth.Requests;

[Collection(AuthTestsCollection.Name)]
public class AuthenticateTests : IAsyncLifetime
{
    private readonly Mock<IClock> _clockMock;
    private readonly CancellationTokenSource _cts = Factory.CreateCancellationTokenSource(60);
    private readonly DatabaseHarness<Program> _database;
    private readonly AuthTestFixture _fixture;
    private readonly HttpClientHarness<Program> _httpClient;

    private AuthOptions _authOptions;
    private IPasswordHasher _passwordHasher;
    private AsyncServiceScope _scope;

    public AuthenticateTests(AuthTestFixture testFixture)
    {
        _clockMock = testFixture.ClockMock;
        _database = testFixture.Database;
        _httpClient = testFixture.HttpClient;
        _fixture = testFixture;
    }

    public async Task InitializeAsync()
    {
        await _database.Clear(_cts.Token);
        _scope = _fixture.Factory.Services.CreateAsyncScope();
        _passwordHasher = _scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        _authOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }

    private async Task<RestResponse<TResponse>> ExecuteAuthenticateRequest<TResponse>(string email, string password)
    {
        var authRequest = new Authenticate.Request(email, password);

        var httpClient = _httpClient.CreateClient();
        var restClient = new RestClient(httpClient);

        var restResponse =
            await restClient.ExecutePostAsync<TResponse>(
                new RestRequest("/auth").AddJsonBody(authRequest));

        return restResponse;
    }

    [Fact]
    private async Task EmailAndPasswordAreValid_ReturnsAccessTokenInBodyAndRefreshTokenInCookies()
    {
        var utcNow = new DateTime(2023, 06, 25, 0, 0, 0, DateTimeKind.Utc);
        _clockMock.SetupGet(clock => clock.UtcNow).Returns(utcNow);

        const string email = "email";
        const string password = "password";

        var user = new User(
            email,
            _passwordHasher.Hash(password),
            null,
            utcNow,
            new[] { Role.User, Role.Analyst });

        await _database.Execute(
            async dbContext =>
            {
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            });

        var restResponse = await ExecuteAuthenticateRequest<Authenticate.Response>(email, password);

        await _database.Execute(
            async dbContext =>
                await restResponse.ShouldBeValidAuthenticateResponse(user, utcNow, _authOptions, dbContext));
    }

    [Fact]
    private async Task EmailDoesNotExist_ReturnsUnauthorisedResponse()
    {
        const string email = "anyEmail";
        const string password = "anyPassword";

        var restResponse = await ExecuteAuthenticateRequest<ProblemDetailsContract>(email, password);

        restResponse.ShouldBeUnauthorizedResponse();
    }

    [Fact]
    private async Task PasswordIsInvalid_ReturnsUnauthorisedResponse()
    {
        var utcNow = new DateTime(2023, 06, 27, 0, 0, 0, DateTimeKind.Utc);
        _clockMock.SetupGet(clock => clock.UtcNow).Returns(utcNow);

        var user = new User(
            "email",
            _passwordHasher.Hash("validPassword"),
            null,
            utcNow,
            new[] { Role.User, Role.Analyst });

        await _database.Execute(
            async dbContext =>
            {
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            });

        var restResponse = await ExecuteAuthenticateRequest<ProblemDetailsContract>(user.Email, "invalidPassword");

        restResponse.ShouldBeUnauthorizedResponse();
    }

    [Fact]
    public async Task RequestValidator_EmailIsEmpty_ReturnsValidationError()
    {
        const string email = "";
        const string password = "anyPassword";

        var restResponse = await ExecuteAuthenticateRequest<ProblemDetailsContract>(email, password);

        restResponse.ShouldBeApiModelValidationFailedResponse("email", "auth_validation_email_is_empty");
    }

    [Fact]
    private async Task RequestValidator_PasswordIsEmpty_ReturnsValidationError()
    {
        const string email = "anyEmail";
        const string password = "";

        var restResponse = await ExecuteAuthenticateRequest<ProblemDetailsContract>(email, password);

        restResponse.ShouldBeApiModelValidationFailedResponse("password", "auth_validation_password_is_empty");
    }
}
