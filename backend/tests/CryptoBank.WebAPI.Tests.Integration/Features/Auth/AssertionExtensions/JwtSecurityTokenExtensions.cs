using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CryptoBank.Domain.Models;
using FluentAssertions;

namespace CryptoBank.WebAPI.Tests.Integration.Features.Auth.AssertionExtensions;

internal static class JwtSecurityTokenExtensions
{
    public static void ShouldContainValidUserId(this JwtSecurityToken jwtAccessToken, User user)
    {
        var userIdFromClaims = jwtAccessToken.Claims
            .SingleOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
            ?.Value;

        userIdFromClaims.Should().Be(user.Id.ToString());
    }

    public static void ShouldContainValidRoles(this JwtSecurityToken jwtAccessToken, User user)
    {
        var rolesFromClaims = jwtAccessToken.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value);

        rolesFromClaims.Should().Equal(user.Roles.Select(role => role.ToString()));
    }
}
