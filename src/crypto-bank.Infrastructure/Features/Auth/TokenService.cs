using System.Security.Claims;
using System.Text;
using crypto_bank.Common;
using crypto_bank.Domain.Authorization;
using crypto_bank.Infrastructure.Features.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace crypto_bank.Infrastructure.Features.Auth;

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
}
