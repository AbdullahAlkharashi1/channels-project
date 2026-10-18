using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Ticketing.Api.Configuration;
using Ticketing.Api.Users;

namespace Ticketing.Api.Auth;

public sealed class JwtTokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _credentials;

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;

        var key = Convert.FromBase64String(_options.Secret);
        if (key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must decode to at least 32 bytes for HMAC-SHA256.");
        }

        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTimeOffset ExpiresAt) Issue(AppUser user)
    {
        var now = _timeProvider.GetUtcNow();
        var expiresAt = now.Add(_options.Ttl);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _credentials,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Username,
                [CurrentUser.UserIdClaim] = user.Id,
                [CurrentUser.RoleClaim] = user.Role.ToString()
            }
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}
