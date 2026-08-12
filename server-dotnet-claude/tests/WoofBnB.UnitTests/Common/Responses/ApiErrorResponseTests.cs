using System.Text.Json;
using WoofBnB.Api.Serialization;
using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Responses;

namespace WoofBnB.UnitTests.Common.Responses;

/// <summary>
/// Asserts parity with server/src/utils/ApiError.js: property order
/// (success, statusCode, message, errors, timestamp), and that `stack` is present only
/// when explicitly set (Node only adds it when NODE_ENV=development), never emitted as
/// a null placeholder.
/// </summary>
public class ApiErrorResponseTests
{
    private static readonly JsonSerializerOptions Options = WoofBnBJsonOptions.CreateDefault();

    [Fact]
    public void Serializes_ValidationFailure_WithFieldMessageErrorsList()
    {
        var response = new ApiErrorResponse
        {
            StatusCode = HttpStatusCodes.BadRequest,
            Message = "Validation Failed",
            Errors = [new ValidationErrorItem { Field = "email", Message = "Invalid email address" }],
            Timestamp = new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc),
            ErrorCode = ErrorCodes.ValidationError,
        };

        var json = JsonSerializer.Serialize(response, Options);

        Assert.Equal(
            "{\"success\":false,\"statusCode\":400,\"message\":\"Validation Failed\"," +
            "\"errors\":[{\"field\":\"email\",\"message\":\"Invalid email address\"}]," +
            "\"timestamp\":\"2026-07-31T09:30:00.000Z\",\"errorCode\":\"VALIDATION_ERROR\"}",
            json);
    }

    [Fact]
    public void Serializes_WithoutStack_OmitsStackKeyEntirely()
    {
        var response = new ApiErrorResponse
        {
            StatusCode = HttpStatusCodes.InternalServerError,
            Message = "An unexpected error occurred.",
            Errors = null,
            Timestamp = new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc),
            ErrorCode = ErrorCodes.InternalServerError,
        };

        var json = JsonSerializer.Serialize(response, Options);

        Assert.DoesNotContain("stack", json);
        Assert.Contains("\"errors\":null", json);
    }

    [Fact]
    public void Serializes_WithStack_IncludesStackKey()
    {
        var response = new ApiErrorResponse
        {
            StatusCode = HttpStatusCodes.InternalServerError,
            Message = "An unexpected error occurred.",
            Timestamp = new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc),
            ErrorCode = ErrorCodes.InternalServerError,
            Stack = "at Foo.Bar()",
        };

        var json = JsonSerializer.Serialize(response, Options);

        Assert.Contains("\"stack\":\"at Foo.Bar()\"", json);
    }
}
