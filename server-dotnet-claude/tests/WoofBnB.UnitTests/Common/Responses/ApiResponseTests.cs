using System.Text.Json;
using WoofBnB.Api.Serialization;
using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Responses;

namespace WoofBnB.UnitTests.Common.Responses;

/// <summary>
/// Asserts byte-for-byte parity with server/src/utils/ApiResponse.js: property order
/// (success, statusCode, message, data, timestamp), camelCase names, and the exact
/// ISO-8601-with-milliseconds timestamp format.
/// </summary>
public class ApiResponseTests
{
    private static readonly JsonSerializerOptions Options = WoofBnBJsonOptions.CreateDefault();

    [Fact]
    public void Serializes_WithNodeCompatiblePropertyOrderAndNames()
    {
        var response = ApiResponse<object>.Create(
            HttpStatusCodes.Created,
            "Pet sitter registered successfully",
            data: new { id = "abc123" },
            timestamp: new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(response, Options);

        Assert.Equal(
            "{\"success\":true,\"statusCode\":201,\"message\":\"Pet sitter registered successfully\"," +
            "\"data\":{\"id\":\"abc123\"},\"timestamp\":\"2026-07-31T09:30:00.000Z\"}",
            json);
    }

    [Fact]
    public void Serializes_NullData_AsExplicitNullNotOmitted()
    {
        var response = ApiResponse<object>.Create(
            HttpStatusCodes.Ok,
            "Pet sitters fetched successfully",
            data: null,
            timestamp: new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(response, Options);

        Assert.Contains("\"data\":null", json);
    }
}
