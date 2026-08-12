namespace WoofBnB.Application.Common.Responses;

/// <summary>
/// Centralized error codes (CLAUDE.md §8). The Node backend has no error-code strings at all —
/// this is an additive field on error responses (decision D-2). Nothing in the approved frontend
/// reads it today, so adding it is non-breaking; it must never replace the existing
/// success/statusCode/message/errors/timestamp envelope fields.
/// </summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string BadRequest = "BAD_REQUEST";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";

    // Emitted only by the login rate limiter. No existing endpoint's error code changes.
    public const string TooManyRequests = "TOO_MANY_REQUESTS";
}
