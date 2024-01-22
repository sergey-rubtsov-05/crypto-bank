using CryptoBank.Common;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Auth;

public class AuthTestFixture : IAsyncLifetime
{
    internal Mock<IClock> ClockMock { get; private set; }
    internal DatabaseHarness<Program> Database { get; private set; }
    internal HttpClientHarness<Program> HttpClient { get; private set; }
    internal WebApplicationFactory<Program> Factory { get; private set; }

    public async Task InitializeAsync()
    {
        ClockMock = new Mock<IClock>();
        ClockMock
            .Setup(clock => clock.UtcNow)
            .Returns(07.October(2023).AsUtc());

        Database = new DatabaseHarness<Program>();
        HttpClient = new HttpClientHarness<Program>();

        Factory = new WebApplicationFactory<Program>()
            .WithHarness(Database)
            .WithHarness(HttpClient)
            .WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureServices(
                        services =>
                        {
                            services.AddSingleton(ClockMock.Object);
                        });
                });

        var cancellationToken = Common.Factories.Factory.CreateCancellationToken(60);

        await Database.Start(Factory, cancellationToken);
        await HttpClient.Start(Factory, cancellationToken);

        var _ = Factory.Server;
    }

    public async Task DisposeAsync()
    {
        await Database.Stop();
        await HttpClient.Stop();
    }
}
