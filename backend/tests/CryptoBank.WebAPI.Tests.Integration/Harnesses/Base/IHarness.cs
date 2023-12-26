using Microsoft.AspNetCore.Mvc.Testing;

namespace CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;

public interface IHarness<TProgram> where TProgram : class
{
    void ConfigureWebHostBuilder(IWebHostBuilder builder);
    Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken = default);
    Task Stop();
}
