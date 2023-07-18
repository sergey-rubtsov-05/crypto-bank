using System.Net;
using FluentAssertions;
using RestSharp;

namespace CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;

internal static class RestResponseExtensions
{
    public static void ShouldBeValidJsonResponse(this RestResponseBase restResponse)
    {
        restResponse.StatusCode.Should().Be(HttpStatusCode.OK, restResponse.Content);
        restResponse.ContentType.Should().Be("application/json");
    }
}
