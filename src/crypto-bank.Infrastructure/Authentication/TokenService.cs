using crypto_bank.Database;
using crypto_bank.Domain.Models;
using crypto_bank.Infrastructure.Features.Users;
using crypto_bank.Infrastructure.Features.Users.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace crypto_bank.Infrastructure.Authentication;

public class TokenService
{
    private readonly TimeSpan _accessTokenLifeTime;
    private readonly CryptoBankDbContext _dbContext;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler;
    private readonly TimeSpan _refreshTokenLifeTime;

    /// <summary>
    ///     TODO: this should be stored in a secure place
    /// </summary>
    private readonly byte[] _secretKeyBytes = "myJwtSecretKey00myJwtSecretKey00"u8.ToArray();

    private readonly UserService _userService;

    public TokenService(UserService userService, IOptions<TokenOptions> tokenOptions, CryptoBankDbContext dbContext)
    {
        _userService = userService;
        _dbContext = dbContext;
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

            var tokenId = Guid.NewGuid();

            var accessToken = CreateAccessToken(tokenId, email);
            var refreshToken = CreateRefreshToken(tokenId, email);

            var token = new Token(tokenId, accessToken, refreshToken);

            await _dbContext.Tokens.AddAsync(token);
            await _dbContext.SaveChangesAsync();

            return token;
        }
        catch (UserNotFoundException userNotFoundException)
        {
            throw new AuthenticationException("Couldn't get the user", userNotFoundException);
        }
    }

    private string CreateAccessToken(Guid tokenId, string email)
    {
        var accessToken = CreateJsonWebToken(_accessTokenLifeTime, tokenId, email);
        return accessToken;
    }

    private string CreateRefreshToken(Guid tokenId, string email)
    {
        var refreshToken = CreateJsonWebToken(_refreshTokenLifeTime, tokenId, email);
        return refreshToken;
    }

    private string CreateJsonWebToken(TimeSpan tokenLifeTime, Guid tokenId, string email)
    {
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.Add(tokenLifeTime),
            Claims = new Dictionary<string, object>
            {
                { JwtRegisteredClaimNames.Jti, tokenId }, { JwtRegisteredClaimNames.Email, email },
            },
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(_secretKeyBytes), SecurityAlgorithms.HmacSha256),
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
