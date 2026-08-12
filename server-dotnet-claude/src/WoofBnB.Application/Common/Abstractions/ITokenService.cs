using WoofBnB.Domain.Entities;

namespace WoofBnB.Application.Common.Abstractions;

/// <summary>
/// Mirrors server/src/modules/auth/auth.token.service.js:generateAccessToken. Actual
/// token validation for protected requests happens inside the JWT bearer authentication
/// pipeline (see WoofBnB.Api's AddJwtBearer configuration), not through this interface —
/// there is no other real caller of "validate a raw token string" in the app, so no
/// abstraction is added here for it (AGENTS.md §25 — no interface without a real need).
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
}
