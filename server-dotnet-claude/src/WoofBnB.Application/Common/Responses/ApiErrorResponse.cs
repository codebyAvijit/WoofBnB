using System.Text.Json.Serialization;

namespace WoofBnB.Application.Common.Responses;

/// <summary>
/// Mirrors server/src/utils/ApiError.js: property order is success, statusCode, message,
/// errors, timestamp, with `stack` appended only when NODE_ENV=development
/// (see server/src/middlewares/error.middleware.js). `errorCode` is an additive field
/// (decision D-2 in the approved migration plan) — the Node backend has no error-code
/// strings at all, so this is a non-breaking addition, never a replacement of the
/// existing fields the frontend already depends on.
/// </summary>
public sealed class ApiErrorResponse
{
    public bool Success { get; init; } = false;

    public required int StatusCode { get; init; }

    public required string Message { get; init; }

    public IReadOnlyList<ValidationErrorItem>? Errors { get; init; }

    public required DateTime Timestamp { get; init; }

    public string? ErrorCode { get; init; }

    /// <summary>
    /// Only populated in Development, matching Node's `if (NODE_ENV === "development") response.stack = err.stack`.
    /// Omitted from the JSON entirely (not emitted as null) when not set, matching Node's behaviour
    /// of never adding the property outside development.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Stack { get; init; }
}
