using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;

public static class ShouldExtensions
{
#nullable enable

    public static AndConstraint<ObjectAssertions> ShouldNotBeNull(
        [NotNull] this object? actualValue,
        string because = "",
        params object[] becauseArgs)
    {
#pragma warning disable CS8777
        return actualValue.Should().NotBeNull(because, becauseArgs);
#pragma warning restore CS8777
    }

#nullable disable
}
