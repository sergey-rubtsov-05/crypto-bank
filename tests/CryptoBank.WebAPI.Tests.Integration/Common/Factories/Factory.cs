using System.Diagnostics;

namespace CryptoBank.WebAPI.Tests.Integration.Common.Factories;

public static class Factory
{
    public static CancellationToken CreateCancellationToken(int timeoutInSeconds = 30) =>
        CreateCancellationTokenSource(timeoutInSeconds).Token;
    
    public static CancellationTokenSource CreateCancellationTokenSource(int timeoutInSeconds = 30) =>
        new CancellationTokenSource(
            Debugger.IsAttached
                ? TimeSpan.FromMinutes(30)
                : TimeSpan.FromSeconds(timeoutInSeconds));
}
