namespace WoofBnB.Application.Common.Responses;

/// <summary>
/// Mirrors server/src/utils/ApiResponse.js exactly: property order is
/// success, statusCode, message, data, timestamp. Do not reorder — property order
/// is part of the byte-for-byte parity contract checked against the Node golden responses.
/// Timestamp is a DateTime (not a pre-formatted string) so the single globally-registered
/// JSON converter (WoofBnB.Api.Serialization.IsoMillisecondDateTimeConverter) is the only
/// place that knows about Node's exact "yyyy-MM-ddTHH:mm:ss.fffZ" format.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; } = true;

    public required int StatusCode { get; init; }

    public required string Message { get; init; }

    public T? Data { get; init; }

    public required DateTime Timestamp { get; init; }

    public static ApiResponse<T> Create(int statusCode, string message, T? data, DateTime timestamp) =>
        new()
        {
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Timestamp = timestamp,
        };
}
