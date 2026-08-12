using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WoofBnB.Infrastructure.Security;

/// <summary>
/// Single source of truth for token validation parameters, used by AddJwtBearer in
/// WoofBnB.Api's Program.cs, so token generation (JwtTokenService) and token validation
/// can never drift apart. Mirrors server/src/modules/auth/auth.token.service.js:
/// jwt.verify checks only signature + exp, with no issuer/audience and (unlike
/// jsonwebtoken, which has no clock-skew concept) no leeway — .NET's 5-minute default
/// ClockSkew is explicitly zeroed to match (audit decision R7).
/// </summary>
public static class JwtTokenValidationParametersFactory
{
    public static TokenValidationParameters Create(JwtOptions options) => new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)),
        ClockSkew = TimeSpan.Zero,
    };
}
