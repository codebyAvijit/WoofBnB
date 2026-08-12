namespace WoofBnB.Api.Middleware;

/// <summary>
/// Reproduces the response headers Node's helmet() actually emits (server/src/app.js:14-20),
/// which ASP.NET does not set by default. Node configures helmet with four protections
/// explicitly disabled:
///
///   contentSecurityPolicy: false, hsts: false, crossOriginOpenerPolicy: false,
///   originAgentCluster: false
///
/// so those four are deliberately NOT set here either — matching the existing deployed
/// behaviour rather than tightening it unilaterally. HSTS in particular is a deployment
/// decision (it depends on whether TLS is terminated here or upstream) and is intentionally
/// left out of this middleware.
///
/// The remaining headers below are the ones helmet sets by default and that carry over
/// safely to a JSON API. They are set before the response body is written, and only when
/// not already present, so an upstream reverse proxy that sets its own values wins.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        // OnStarting rather than setting headers inline: ExceptionHandlingMiddleware calls
        // Response.Clear() when it converts an exception into an error envelope, which would
        // discard headers written earlier in the pipeline. This callback runs after that,
        // immediately before the first byte is sent, so error responses are covered too.
        context.Response.OnStarting(static state =>
        {
            var headers = ((HttpContext)state).Response.Headers;

            // Stop browsers MIME-sniffing a response into a different content type.
            headers.TryAdd("X-Content-Type-Options", "nosniff");

            // Legacy clickjacking protection. Harmless for an API and part of helmet's
            // default set; modern browsers prefer CSP frame-ancestors, which Node disables.
            headers.TryAdd("X-Frame-Options", "SAMEORIGIN");

            // Do not leak the full API URL (including query string, e.g. nearby lat/lng)
            // to third-party origins via the Referer header.
            headers.TryAdd("Referrer-Policy", "no-referrer");

            // Opt out of Chrome's FLoC/Topics-style interest cohort calculation.
            headers.TryAdd("Cross-Origin-Resource-Policy", "same-origin");

            // helmet sets this to disable legacy IE content-type sniffing behaviour.
            headers.TryAdd("X-DNS-Prefetch-Control", "off");

            return Task.CompletedTask;
        }, context);

        return _next(context);
    }
}
