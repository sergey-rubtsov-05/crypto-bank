using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CryptoBank.Common;
using CryptoBank.Domain.Authorization;
using CryptoBank.WebAPI.Features.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CryptoBank.WebAPI.Features.Auth.Services;

public class TokenService
{
    private readonly IClock _clock;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler;
    private readonly JwtOptions _jwtOptions;

    public TokenService(IOptions<AuthOptions> authOptions, IClock clock)
    {
        _clock = clock;
        _jsonWebTokenHandler = new JsonWebTokenHandler();
        _jwtOptions = authOptions.Value.Jwt;
    }

    public string CreateAccessToken(int userId, Role[] roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = _clock.UtcNow.Add(_jwtOptions.AccessTokenLifeTime),
            SigningCredentials = signingCredentials,
            Claims = new Dictionary<string, object>
            {
                { ClaimTypes.NameIdentifier, userId },
                { ClaimTypes.Role, roles.Select(role => role.ToString()).ToArray() },
            },
        };

        var accessToken = _jsonWebTokenHandler.CreateToken(securityTokenDescriptor);

        return accessToken;
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
