using crypto_bank.Domain.Models;
using crypto_bank.Infrastructure.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace crypto_bank.Infrastructure;

public class TokenService
{
    /// <summary>
    ///     TODO: this should be stored in configuration
    /// </summary>
    private readonly TimeSpan _accessTokenLifeTime;

    private readonly JsonWebTokenHandler _jsonWebTokenHandler;

    /// <summary>
    ///     TODO: this should be stored in configuration
    /// </summary>
    private readonly TimeSpan _refreshTokenLifeTime;

    /// <summary>
    ///     TODO: this should be stored in a secure place
    /// </summary>
    private readonly byte[] _secretKeyBytes = "myJwtSecretKey00myJwtSecretKey00"u8.ToArray();

    private readonly UserService _userService;

    public TokenService(UserService userService)
    {
        _userService = userService;
        _jsonWebTokenHandler = new JsonWebTokenHandler();
        _accessTokenLifeTime = TimeSpan.FromMinutes(5);
        _refreshTokenLifeTime = TimeSpan.FromMinutes(60);
    }

    public async Task<Token> Create(string email, string password)
    {
        try
        {
            var user = await _userService.Get(email);

            if (!user.Password.Equals(password, StringComparison.Ordinal))
                throw new AuthenticationException("Invalid password");

            var accessToken = CreateAccessToken();
            var refreshToken = CreateRefreshToken();
            //todo save to db
            //todo save as family of tokens

            return new Token(accessToken, refreshToken);
        }
        catch (GettingUserException gettingUserException)
        {
            throw new AuthenticationException("Couldn't get the user", gettingUserException);
        }
    }

    private string CreateAccessToken()
    {
        var accessToken = CreateJsonWebToken(_accessTokenLifeTime);
        return accessToken;
    }

    private string CreateRefreshToken()
    {
        var refreshToken = CreateJsonWebToken(_refreshTokenLifeTime);
        return refreshToken;
    }

    private string CreateJsonWebToken(TimeSpan tokenLifeTime)
    {
        //todo token have to store information about user
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.Add(tokenLifeTime),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(_secretKeyBytes), SecurityAlgorithms.HmacSha256),
        };
        var accessToken = _jsonWebTokenHandler.CreateToken(securityTokenDescriptor);
        return accessToken;
    }
}
