using System.Text.Json;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Responses;

namespace WoofBnB.Api.Middleware;

/// <summary>
/// Shared by ExceptionHandlingMiddleware and the JWT bearer OnChallenge/OnForbidden events
/// (WoofBnB.Api's Program.cs) — both need to write the exact same ApiErrorResponse envelope,
/// and authentication failures never throw an AppException (they short-circuit the pipeline
/// before any controller/middleware exception handling runs), so this is genuinely shared
/// logic, not premature abstraction.
/// </summary>
public static class ApiErrorResponseWriter
{
    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string message,
        string errorCode,
        IReadOnlyList<ValidationErrorItem>? errors,
        IClock clock,
        JsonSerializerOptions jsonOptions,
        IHostEnvironment environment,
        Exception? exception = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            Timestamp = clock.UtcNow,
            ErrorCode = errorCode,
            Stack = environment.IsDevelopment() ? exception?.ToString() : null,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
