using System.Diagnostics;

namespace WoofBnB.Api.Middleware;

/// <summary>
/// Structured request logging with a correlation id, using the built-in ILogger rather than
/// adding a logging framework dependency: the values below are emitted as named properties,
/// so a structured sink (if one is ever configured) captures them as fields, while the
/// default console provider still renders a readable line.
///
/// Deliberately minimal about what it records. The path is logged but the query string is
/// NOT, because /api/petsitters/nearby carries the user's real latitude and longitude —
/// precise location is personal data and does not belong in application logs. Request and
/// response bodies are never logged either: they carry passwords on /api/auth/login and
/// personal contact details on /api/petsitters.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Reuse an upstream id when a proxy or the caller already established one, so a
        // request can be traced across hops; otherwise start a new one here.
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var incoming)
            && !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : context.TraceIdentifier;

        context.Response.OnStarting(static state =>
        {
            var (ctx, id) = ((HttpContext, string))state;
            ctx.Response.Headers.TryAdd(CorrelationIdHeader, id);
            return Task.CompletedTask;
        }, (context, correlationId));

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
        });

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
