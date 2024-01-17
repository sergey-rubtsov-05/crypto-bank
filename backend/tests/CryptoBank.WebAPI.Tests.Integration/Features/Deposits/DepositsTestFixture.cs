using CryptoBank.Common;
using CryptoBank.WebAPI.Tests.Integration.Harnesses;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using CryptoBank.WebAPI.Tests.Integration.Harnesses.Bitcoin;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits;

public class DepositsTestFixture : IAsyncLifetime
{
    internal readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(1);

    internal Mock<IClock> ClockMock { get; private set; }
    internal DatabaseHarness<Program> Database { get; private set; }
    internal HttpClientHarness<Program> HttpClient { get; private set; }
    internal BitcoinHarness<Program> Bitcoin { get; private set; }
    internal WebApplicationFactory<Program> Factory { get; private set; }

    public async Task InitializeAsync()
    {
        ClockMock = new Mock<IClock>();
        ClockMock
            .Setup(clock => clock.UtcNow)
            .Returns(07.October(2023).AsUtc());

        Bitcoin = new BitcoinHarness<Program>();
        Database = new DatabaseHarness<Program>();
        HttpClient = new HttpClientHarness<Program>();

        Factory = new WebApplicationFactory<Program>()
            .WithHarness(Bitcoin)
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

                    builder.ConfigureAppConfiguration(
                        (_, configBuilder) =>
                        {
                            configBuilder.AddInMemoryCollection(
                                new Dictionary<string, string>
                                {
                                    { "Features:Deposits:BitcoinBlockchainScanInterval", ScanInterval.ToString() },
                                });
                        });
                });

        var cancellationToken = Common.Factories.Factory.CreateCancellationToken(60);

        await Bitcoin.Start(Factory, cancellationToken);
        await Database.Start(Factory, cancellationToken);
        await HttpClient.Start(Factory, cancellationToken);

        var _ = Factory.Server;
    }

    public async Task DisposeAsync()
    {
        await Database.Stop();
        await Bitcoin.Stop();
        await HttpClient.Stop();
    }
}
