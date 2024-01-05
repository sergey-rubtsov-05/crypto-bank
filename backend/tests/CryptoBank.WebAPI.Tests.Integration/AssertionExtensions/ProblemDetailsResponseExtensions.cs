using System.Net;
using CryptoBank.WebAPI.Tests.Integration.Common.Errors;
using FluentAssertions;
using RestSharp;

namespace CryptoBank.WebAPI.Tests.Integration.AssertionExtensions;

internal static class ProblemDetailsResponseExtensions
{
    public static void ShouldBeApiModelValidationFailedResponse(
        this RestResponse<ProblemDetailsContract> restResponse,
        string expectedField,
        string expectedCode)
    {
        const HttpStatusCode expectedHttpStatusCode = HttpStatusCode.BadRequest;
        restResponse.StatusCode.Should().Be(expectedHttpStatusCode);
        restResponse.ContentType.Should().Be("application/problem+json");

        var problemDetails = restResponse.Data;

        problemDetails.Should().NotBeNull();
        problemDetails.Title.Should().Be("Api model validation failed");
        problemDetails.Detail.Should().Be("One or more validation errors have occurred");
        problemDetails.Status.Should().Be((int)expectedHttpStatusCode);

        var actualErrors = problemDetails.Errors;
        actualErrors.Should().NotBeNull();
        actualErrors.Should().ContainSingle();

        var actualError = actualErrors.Single();
        actualError.Field.Should().BeEquivalentTo(expectedField);
        actualError.Code.Should().Be(expectedCode);
    }

    public static void ShouldBeUnauthorizedResponse(this RestResponse<ProblemDetailsContract> restResponse)
    {
        const HttpStatusCode expectedHttpStatusCode = HttpStatusCode.Unauthorized;

        restResponse.StatusCode.Should().Be(expectedHttpStatusCode);
        restResponse.ContentType.Should().Be("application/problem+json");

        var problemDetails = restResponse.Data;

        problemDetails.Should().NotBeNull();
        problemDetails.Title.Should().Be("Authentication failed");
        problemDetails.Status.Should().Be((int)expectedHttpStatusCode);
    }
}
