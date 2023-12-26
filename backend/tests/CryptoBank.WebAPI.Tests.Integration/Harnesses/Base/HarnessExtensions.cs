using Microsoft.AspNetCore.Mvc.Testing;

namespace CryptoBank.WebAPI.Tests.Integration.Harnesses.Base;

public static class HarnessExtensions
{
    public static WebApplicationFactory<T> WithHarness<T>(this WebApplicationFactory<T> factory, IHarness<T> harness)
        where T : class
    {
        return factory.WithWebHostBuilder(harness.ConfigureWebHostBuilder);
    }
}
