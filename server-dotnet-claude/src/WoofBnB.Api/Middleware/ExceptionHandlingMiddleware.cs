using System.Text.Json;
using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Application.Common.Responses;

namespace WoofBnB.Api.Middleware;

/// <summary>
/// Central error-to-response mapping, mirroring server/src/middlewares/error.middleware.js:
/// an AppException carries an explicit status/message/errorCode (and optional field errors)
/// and is rendered as-is; anything else is an unexpected exception.
///
/// Deliberate deviation from Node (CLAUDE.md §9: "Never expose internal exception details in
/// production responses"): Node's error middleware uses `err.message` for *any* thrown error,
/// including unexpected ones, which can leak internal detail. This middleware always returns
/// a generic message and error code for unexpected exceptions and never the raw
/// Exception.Message. The dev-only `stack` field is still reproduced, matching Node's
/// `if (NODE_ENV === "development") response.stack = err.stack`.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IClock _clock;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment,
        IClock clock,
        JsonSerializerOptions jsonOptions)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _clock = clock;
        _jsonOptions = jsonOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Application exception occurred.");

            await WriteErrorResponseAsync(context, ex.StatusCode, ex.Message, ex.ErrorCode, ex.Errors, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            await WriteErrorResponseAsync(
                context,
                HttpStatusCodes.InternalServerError,
                "An unexpected error occurred.",
                ErrorCodes.InternalServerError,
                errors: null,
                ex);
        }
    }

    private async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message,
        string errorCode,
        IReadOnlyList<ValidationErrorItem>? errors,
        Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "Response already started; cannot write error envelope for {ExceptionType}.",
                exception.GetType().Name);

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
            Timestamp = _clock.UtcNow,
            ErrorCode = errorCode,
            Stack = _environment.IsDevelopment() ? exception.ToString() : null,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));
    }
}
