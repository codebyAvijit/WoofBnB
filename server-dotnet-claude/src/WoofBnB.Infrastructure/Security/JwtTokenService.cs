using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Domain.Entities;

namespace WoofBnB.Infrastructure.Security;

/// <summary>
/// Mirrors server/src/modules/auth/auth.token.service.js:generateAccessToken. The payload
/// is built directly as a JwtPayload dictionary — deliberately NOT via
/// JwtSecurityTokenHandler.CreateToken(SecurityTokenDescriptor), which was found (by an
/// actual failing test, not by inspection) to unconditionally add an "nbf" claim set to
/// the real wall clock whenever Expires is set, even with no NotBefore requested. That
/// silently changes the token shape versus Node — jsonwebtoken's jwt.sign never adds nbf
/// unless explicitly configured to. Building the payload directly, and avoiding a
/// ClaimsIdentity (which would go through JwtSecurityTokenHandler's static
/// DefaultOutboundClaimTypeMap and silently rewrite short claim names like "role" to long
/// ClaimTypes URIs — audit decision R7), the resulting payload is exactly
/// { id, role, iat, exp } — matching jsonwebtoken's jwt.sign({ id, role }, secret,
/// { expiresIn }) claim-for-claim.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var payload = new JwtPayload
        {
            ["id"] = user.Id.ToString(),
            ["role"] = user.Role,
            ["iat"] = EpochTime.GetIntDate(_clock.UtcNow),
            ["exp"] = EpochTime.GetIntDate(_clock.UtcNow.AddMinutes(_options.ExpiresInMinutes)),
        };

        var token = new JwtSecurityToken(new JwtHeader(signingCredentials), payload);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
