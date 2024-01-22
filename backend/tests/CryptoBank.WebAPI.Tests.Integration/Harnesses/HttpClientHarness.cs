using CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CryptoBank.WebAPI.Tests.Integration.Harnesses;

internal class HttpClientHarness<T> : IHarness<T> where T : class
{
    private WebApplicationFactory<T> _factory;
    private bool _started;

    public void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
    }

    public Task Start(WebApplicationFactory<T> factory, CancellationToken cancellationToken = default)
    {
        _factory = factory;
        _started = true;

        return Task.CompletedTask;
    }

    public Task Stop()
    {
        _started = false;

        return Task.CompletedTask;
    }

    public HttpClient CreateClient()
    {
        ThrowIfNotStarted();

        return _factory.CreateClient();
    }

    private void ThrowIfNotStarted()
    {
        if (!_started)
            throw new InvalidOperationException($"HTTP client harness is not started. Call {nameof(Start)} first.");
    }
}
