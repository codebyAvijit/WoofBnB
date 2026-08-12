using Microsoft.AspNetCore.Authorization;

namespace WoofBnB.Api.Authorization;

/// <summary>
/// Marker requirement for the "ActiveUser" policy. Separating this from JWT bearer
/// *authentication* (401: missing/invalid/expired token, or a deleted user) is what lets
/// a valid-but-disabled account fail *authorization* instead (403), matching
/// server/src/middlewares/auth.middleware.js's `if (!user.isActive) throw 403` exactly.
/// </summary>
public sealed class ActiveUserRequirement : IAuthorizationRequirement;
