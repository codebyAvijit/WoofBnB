namespace WoofBnB.Api;

/// <summary>
/// Named rate-limiting policies, referenced by both Program.cs (registration) and the
/// controller action they apply to, so the two can never drift apart via a typo'd string.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Applied to POST /api/auth/login only — the sole unauthenticated endpoint that
    /// accepts credentials and is therefore the only online brute-force target.
    /// </summary>
    public const string Login = "login";
}
