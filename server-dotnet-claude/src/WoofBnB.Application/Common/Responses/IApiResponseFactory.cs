namespace WoofBnB.Application.Common.Responses;

/// <summary>
/// Builds success envelopes using IClock (rather than DateTime.UtcNow directly) so the
/// `timestamp` field is deterministic and testable, matching Node's per-response
/// `new Date().toISOString()` call in server/src/utils/ApiResponse.js.
/// </summary>
public interface IApiResponseFactory
{
    ApiResponse<T> Success<T>(int statusCode, string message, T? data);
}
