using crypto_bank.Domain.Models;
using crypto_bank.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace crypto_bank.Infrastructure.Authentication;

public class TokenService
{
    private readonly TimeSpan _accessTokenLifeTime;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler;
    private readonly TimeSpan _refreshTokenLifeTime;

    /// <summary>
    ///     TODO: this should be stored in a secure place
    /// </summary>
    private readonly byte[] _secretKeyBytes = "myJwtSecretKey00myJwtSecretKey00"u8.ToArray();

    private readonly UserService _userService;

    public TokenService(UserService userService, IOptions<TokenOptions> tokenOptions)
    {
        _userService = userService;
        _jsonWebTokenHandler = new JsonWebTokenHandler();
        _accessTokenLifeTime = tokenOptions.Value.AccessTokenLifeTime;
        _refreshTokenLifeTime = tokenOptions.Value.RefreshTokenLifeTime;
    }

    public async Task<Token> Create(string email, string password)
    {
        try
        {
            var user = await _userService.Get(email);

            if (!user.Password.Equals(password, StringComparison.Ordinal))
                throw new AuthenticationException("Invalid password");

            var accessToken = CreateAccessToken(email);
            var refreshToken = CreateRefreshToken(email);
            //todo save to db
            //todo save as family of tokens

            return new Token(accessToken, refreshToken);
        }
        catch (UserNotFoundException userNotFoundException)
        {
            throw new AuthenticationException("Couldn't get the user", userNotFoundException);
        }
    }

    private string CreateAccessToken(string email)
    {
        var accessToken = CreateJsonWebToken(_accessTokenLifeTime, email);
        return accessToken;
    }

    private string CreateRefreshToken(string email)
    {
        var refreshToken = CreateJsonWebToken(_refreshTokenLifeTime, email);
        return refreshToken;
    }

    private string CreateJsonWebToken(TimeSpan tokenLifeTime, string email)
    {
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.Add(tokenLifeTime),
            Claims = new Dictionary<string, object> { { JwtRegisteredClaimNames.Email, email } },
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(_secretKeyBytes), SecurityAlgorithms.HmacSha256),
        };
        var accessToken = _jsonWebTokenHandler.CreateToken(securityTokenDescriptor);
        return accessToken;
    }
}
