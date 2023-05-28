using System.Text;
using crypto_bank.Infrastructure.Features.Auth.Exceptions;
using crypto_bank.Infrastructure.Features.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace crypto_bank.Infrastructure.Features.Auth;

public class TokenService
{
    private readonly TimeSpan _accessTokenLifeTime;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler;
    private readonly byte[] _secretKeyBytes;

    public TokenService(IOptions<AuthOptions> authOptions)
    {
        _jsonWebTokenHandler = new JsonWebTokenHandler();
        _accessTokenLifeTime = authOptions.Value.Jwt.AccessTokenLifeTime;
        _secretKeyBytes = Encoding.UTF8.GetBytes(authOptions.Value.Jwt.SigningKey);
    }

    public string CreateAccessToken()
    {
        var key = new SymmetricSecurityKey(_secretKeyBytes);
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.Add(_accessTokenLifeTime),
            Claims = new Dictionary<string, object>(),
            SigningCredentials = signingCredentials,
        };
        var accessToken = _jsonWebTokenHandler.CreateToken(securityTokenDescriptor);

        return accessToken;
    }

    public async Task Validate(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(_secretKeyBytes),
            ValidateAudience = false,
            ValidateIssuer = false,
        };
        var validationResult = await _jsonWebTokenHandler.ValidateTokenAsync(token, tokenValidationParameters);

        if (validationResult.IsValid)
            return;

        throw new AuthenticationException("Invalid token");
    }
}
