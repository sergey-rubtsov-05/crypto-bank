using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Web;
using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Auth.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestSharp;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Auth.AssertionExtensions;

internal static class AuthenticateResponseExtensions
{
    public static async Task ShouldBeValidAuthenticateResponse(
        this RestResponse<Authenticate.Response> restResponse,
        User user,
        DateTime utcNow,
        AuthOptions authOptions,
        CryptoBankDbContext dbContext)
    {
        restResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        restResponse.ContentType.Should().Be("application/json");

        restResponse.Data.ShouldHaveValidAccessToken(user, authOptions);

        await restResponse.Cookies.ShouldHaveValidRefreshToken(utcNow, authOptions, dbContext);
    }

    private static void ShouldHaveValidAccessToken(
        this Authenticate.Response response,
        User user,
        AuthOptions authOptions)
    {
        var (accessToken, _) = response;

        accessToken.Should().NotBeNullOrWhiteSpace();

        var jwtAccessToken = ValidateAccessToken(authOptions, accessToken);

        jwtAccessToken.ShouldContainValidUserId(user);
        jwtAccessToken.ShouldContainValidRoles(user);
    }

    private static JwtSecurityToken ValidateAccessToken(AuthOptions authOptions, string accessToken)
    {
        //TODO: DRY problem: this code duplicate code from Program.cs
        new JwtSecurityTokenHandler().ValidateToken(
            accessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authOptions.Jwt.Issuer,
                ValidAudience = authOptions.Jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.SigningKey)),
            },
            out var validatedToken);

        validatedToken.Should().NotBeNull();
        validatedToken.Should().BeOfType<JwtSecurityToken>();
        var jwtAccessToken = (JwtSecurityToken)validatedToken;
        return jwtAccessToken;
    }

    public static async Task ShouldHaveValidRefreshToken(
        this CookieCollection cookies,
        DateTime utcNow,
        AuthOptions authOptions,
        CryptoBankDbContext dbContext)
    {
        //todo: problem: test server does not set cookies when HttpOnly is true and Secure is true
        return;

#pragma warning disable CS0162
        var refreshTokenCookie = cookies?.SingleOrDefault(cookie => cookie.Name == "refresh-token");
        refreshTokenCookie.Should().NotBeNull();
        refreshTokenCookie!.HttpOnly.Should().BeTrue();
        refreshTokenCookie.Secure.Should().BeTrue();

        var refreshToken = HttpUtility.UrlDecode(refreshTokenCookie.Value);

        refreshToken.Should().NotBeNullOrWhiteSpace();
        var refreshTokenEntity =
            await dbContext.Tokens.SingleOrDefaultAsync(token => token.RefreshToken == refreshToken);

        refreshTokenEntity.Should().NotBeNull();
        refreshTokenEntity.ExpirationTime.Should().Be(utcNow.Add(authOptions.RefreshTokenLifeTime));
#pragma warning restore CS0162
    }
}
