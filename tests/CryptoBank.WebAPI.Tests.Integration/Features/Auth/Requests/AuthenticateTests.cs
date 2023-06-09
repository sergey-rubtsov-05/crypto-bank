using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Auth.Requests;
using CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using CryptoBank.WebAPI.Tests.Integration.Features.Auth.AssertionExtensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using RestSharp;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Auth.Requests;

public class AuthenticateTests : IAsyncLifetime
{
    private readonly Mock<IClock> _clockMock;
    private readonly WebApplicationFactory<Program> _factory;
    private AuthOptions _authOptions;
    private CryptoBankDbContext _dbContext;
    private AsyncServiceScope _scope;

    public AuthenticateTests()
    {
        _clockMock = new Mock<IClock>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder => builder
                    .ConfigureServices(
                        services =>
                        {
                            services.AddSingleton(_clockMock.Object);
                        })
                    .ConfigureAppConfiguration(
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

    public Task InitializeAsync()
    {
        var _ = _factory.Server;
        _scope = _factory.Services.CreateAsyncScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<CryptoBankDbContext>();
        _authOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Users.ExecuteDeleteAsync();
        await _factory.DisposeAsync();
        await _scope.DisposeAsync();
    }

    private async Task<RestResponse<TResponse>> ExecuteAuthenticateRequest<TResponse>(string email, string password)
    {
        var authRequest = new Authenticate.Request(email, password);

        var httpClient = _factory.CreateClient();
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
            _scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash(password),
            null,
            utcNow,
            new[] { Role.User, Role.Analyst });

        await _dbContext.Users.AddAsync(user);

        await _dbContext.SaveChangesAsync();

        var restResponse = await ExecuteAuthenticateRequest<Authenticate.Response>(email, password);

        await restResponse.ShouldBeValidAuthenticateResponse(user, utcNow, _authOptions, _dbContext);
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
            _scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash("validPassword"),
            null,
            utcNow,
            new[] { Role.User, Role.Analyst });

        await _dbContext.Users.AddAsync(user);

        await _dbContext.SaveChangesAsync();

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
