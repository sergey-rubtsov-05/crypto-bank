using Microsoft.AspNetCore.Mvc.Testing;

namespace CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;

public interface IHarness<TProgram> where TProgram : class
{
    void ConfigureWebHostBuilder(IWebHostBuilder builder);
    Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken = default);

    //todo: question: I think Stop does not need to use CancellationToken. Because it have to stop anyway.
    Task Stop();
}
