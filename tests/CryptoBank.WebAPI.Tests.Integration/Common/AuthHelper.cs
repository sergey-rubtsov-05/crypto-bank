using System.Security.Claims;
using System.Text;
using CryptoBank.Common;
using CryptoBank.Domain.Authorization;
using CryptoBank.WebAPI.Features.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CryptoBank.WebAPI.Tests.Integration.Common;

public class AuthHelper
{
    private readonly IClock _clock;
    private readonly JwtOptions _jwtOptions;

    public AuthHelper(IClock clock, IOptions<AuthOptions> authOptions)
    {
        _clock = clock;
        _jwtOptions = authOptions.Value.Jwt;
    }

    public string CreateAccessToken(int userId, Role[] roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var utcNow = _clock.UtcNow;
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = utcNow.Add(_jwtOptions.AccessTokenLifeTime),
            NotBefore = utcNow,
            SigningCredentials = signingCredentials,
            Claims = new Dictionary<string, object>
            {
                { ClaimTypes.NameIdentifier, userId },
                { ClaimTypes.Role, roles.Select(role => role.ToString()).ToArray() },
            },
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(securityTokenDescriptor);

        return accessToken;
    }
}