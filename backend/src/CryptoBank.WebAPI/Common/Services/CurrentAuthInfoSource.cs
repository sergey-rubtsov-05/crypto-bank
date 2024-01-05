using System.Security.Claims;
using CryptoBank.WebAPI.Features.Auth.Exceptions;

namespace CryptoBank.WebAPI.Common.Services;

public class CurrentAuthInfoSource
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentAuthInfoSource(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserId()
    {
        var nameIdentifier = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(nameIdentifier, out var userId))
            return userId;

        throw new AuthenticationException("Could not get user id from claims.");
    }
}
