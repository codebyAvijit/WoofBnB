using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using WoofBnB.Api;
using WoofBnB.Api.Authorization;
using WoofBnB.Api.Filters;
using WoofBnB.Api.Middleware;
using WoofBnB.Api.Serialization;
using WoofBnB.Api.Swagger;
using WoofBnB.Application.Auth;
using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Responses;
using WoofBnB.Application.PetSitters;
using WoofBnB.Infrastructure.Persistence;
using WoofBnB.Infrastructure.Repositories;
using WoofBnB.Infrastructure.Security;
using WoofBnB.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICES
// ============================================================

const string ClientCorsPolicy = "ClientApp";

builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options => WoofBnBJsonOptions.Apply(options.JsonSerializerOptions));

// Model-binding failures must not produce ASP.NET's default ProblemDetails response —
// ValidationFilter (via FluentValidation) is the only source of 400s, so its envelope
// is the only one clients ever see (CLAUDE.md §10, AGENTS.md §5-6).
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);

// Single JsonSerializerOptions instance shared by MVC responses and
// ExceptionHandlingMiddleware's manually-written error responses, so both surfaces
// serialize identically (camelCase, explicit nulls, Node-compatible DateTime format).
builder.Services.AddSingleton(WoofBnBJsonOptions.CreateDefault());

builder.Services.AddValidatorsFromAssemblyContaining<ApiResponseFactory>();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IApiResponseFactory, ApiResponseFactory>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing required configuration: ConnectionStrings:DefaultConnection.");

// EnableRetryOnFailure covers transient SQL faults (network blips, Azure SQL throttling)
// that would otherwise surface to the client as an unhandled 500. Safe here because the
// application issues no explicit/user-initiated transactions — if one is ever added, it
// must be wrapped in an execution strategy (DbContext.Database.CreateExecutionStrategy)
// or EF will throw. Retries are invisible to the API contract: the duplicate-email path
// still surfaces as a DbUpdateException wrapping SqlException 2601/2627, which
// PetSitterRepository converts to the same 409 as before.
builder.Services.AddDbContext<WoofBnBDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.UseNetTopologySuite();
        sql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));

// ---- Authentication (mirrors server/src/modules/auth/**) ----

// Cheap presence check only — fails fast on a deployment that never configured a
// secret at all. Deliberately NOT used to build TokenValidationParameters below: that
// must read JwtOptions lazily through DI (see AddOptions<JwtBearerOptions>().Configure
// below), because a value captured here, in a local variable, is frozen at this point
// in startup — before WebApplicationFactory-based tests get a chance to layer their own
// config override on top, which silently made every integration test validate against
// the wrong secret until this was caught by a failing test.
var configuredJwtSecret = builder.Configuration[$"{JwtOptions.SectionName}:Secret"];

if (string.IsNullOrWhiteSpace(configuredJwtSecret))
{
    throw new InvalidOperationException("Missing required configuration: Jwt:Secret.");
}

// ---- Production configuration validation ----
//
// Deliberately gated to non-Development: WoofBnBApiFactory/AuthTestApiFactory boot this
// exact Program.cs with UseEnvironment("Development"), and the Phase 8 parity harness runs
// Development too, so applying these in Development would break both. These guard the
// failure modes that are silent at startup and only surface later, in production:
//
//  - A blank Cors:ClientUrl leaves the CORS policy with zero configured origins. The app
//    starts healthy and every browser request from the real frontend is then blocked, with
//    nothing in the logs explaining why. Failing fast is far cheaper to diagnose.
//  - HMAC-SHA256 requires a key of at least 256 bits. A shorter secret passes the presence
//    check above and only throws later, at first token signing — i.e. on the first login
//    attempt, not at deploy time.
if (!builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(builder.Configuration["Cors:ClientUrl"]))
    {
        throw new InvalidOperationException(
            "Missing required configuration: Cors:ClientUrl. Without it no browser origin is "
            + "permitted and all frontend requests will be blocked by CORS.");
    }

    const int MinimumJwtSecretBytes = 32;

    if (System.Text.Encoding.UTF8.GetByteCount(configuredJwtSecret) < MinimumJwtSecretBytes)
    {
        throw new InvalidOperationException(
            $"Invalid configuration: Jwt:Secret must be at least {MinimumJwtSecretBytes} bytes "
            + "for HMAC-SHA256 signing.");
    }
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddScoped<IPetSitterRepository, PetSitterRepository>();
builder.Services.AddScoped<IPetSitterService, PetSitterService>();
builder.Services.AddSingleton<IPasswordHasher>(_ =>
    new BCryptPasswordHasher(builder.Configuration.GetValue("Security:BcryptWorkFactor", 10)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
    {
        options.TokenValidationParameters = JwtTokenValidationParametersFactory.Create(jwtOptionsAccessor.Value);

        // Preserve "id"/"role" as literal claim types on the ClaimsPrincipal — without
        // this, JwtSecurityTokenHandler's default inbound map silently rewrites "role"
        // to a long ClaimTypes URI, breaking FindFirst("role") (audit decision R7).
        options.MapInboundClaims = false;

        options.Events = new JwtBearerEvents
        {
            // Mirrors server/src/middlewares/auth.middleware.js: re-load the user on
            // every request and fail if they no longer exist. Whether they're still
            // active is deferred to the "ActiveUser" authorization policy below, so a
            // disabled account can be distinguished as 403 rather than folded into 401.
            OnTokenValidated = async context =>
            {
                var idClaim = context.Principal?.FindFirst("id")?.Value;

                if (!Guid.TryParse(idClaim, out var userId))
                {
                    context.Fail("User not found");
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = await userRepository.GetByIdAsync(userId);

                if (user is null)
                {
                    context.Fail("User not found");
                    return;
                }

                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                identity.AddClaim(new Claim("is_active", user.IsActive ? "true" : "false"));
            },

            // 401 — mirrors the three failure cases server/src/middlewares/auth.middleware.js
            // can produce: missing/malformed header ("Authentication required"), an
            // invalid/expired/malformed token (Node 500s here today — a bug; this fixes
            // it to 401, decision D-3), or a deleted user ("User not found", surfaced via
            // OnTokenValidated's context.Fail above — matched on message rather than
            // exception type, since a malformed (not merely expired/bad-signature) token
            // throws a different IdentityModel exception type than an expired one, and
            // enumerating every subtype is more fragile than checking for our own
            // deliberately-thrown message).
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var message = context.AuthenticateFailure switch
                {
                    null => "Authentication required",
                    { Message: "User not found" } => "User not found",
                    _ => "Invalid or expired token",
                };

                var services = context.HttpContext.RequestServices;

                await ApiErrorResponseWriter.WriteAsync(
                    context.HttpContext,
                    HttpStatusCodes.Unauthorized,
                    message,
                    ErrorCodes.Unauthorized,
                    errors: null,
                    services.GetRequiredService<IClock>(),
                    services.GetRequiredService<JsonSerializerOptions>(),
                    services.GetRequiredService<IHostEnvironment>());
            },

            // 403 — mirrors auth.middleware.js's `if (!user.isActive) throw 403`.
            OnForbidden = async context =>
            {
                var services = context.HttpContext.RequestServices;

                await ApiErrorResponseWriter.WriteAsync(
                    context.HttpContext,
                    HttpStatusCodes.Forbidden,
                    "Your account has been disabled",
                    ErrorCodes.Forbidden,
                    errors: null,
                    services.GetRequiredService<IClock>(),
                    services.GetRequiredService<JsonSerializerOptions>(),
                    services.GetRequiredService<IHostEnvironment>());
            },
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("ActiveUser", policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new ActiveUserRequirement())));

builder.Services.AddSingleton<IAuthorizationHandler, ActiveUserAuthorizationHandler>();

// Brute-force protection for the one unauthenticated, credential-accepting endpoint.
// Applied as a named policy on AuthController.Login only — deliberately not global, so
// the public browse/nearby/register endpoints the frontend depends on are unaffected.
//
// The limit is per client IP and is set high enough that no legitimate user reaches it,
// while still bounding an online password-guessing attempt. It is raised sharply in
// Development because the Phase 8 parity harness fires many login scenarios back to back
// from a single address and must not start receiving 429s.
builder.Services.AddRateLimiter(options =>
{
    var loginPermitLimit = builder.Environment.IsDevelopment() ? 1000 : 10;

    options.AddPolicy(RateLimitPolicies.Login, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = loginPermitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    // A rejected request must still return the standard envelope rather than an empty
    // 429 body, so the frontend's `error.response.data.message` toast has something to
    // show, consistent with every other error surface.
    options.OnRejected = async (context, cancellationToken) =>
    {
        var services = context.HttpContext.RequestServices;

        await ApiErrorResponseWriter.WriteAsync(
            context.HttpContext,
            HttpStatusCodes.TooManyRequests,
            "Too many login attempts. Please try again later.",
            ErrorCodes.TooManyRequests,
            errors: null,
            services.GetRequiredService<IClock>(),
            services.GetRequiredService<JsonSerializerOptions>(),
            services.GetRequiredService<IHostEnvironment>());
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy =>
    {
        var clientUrl = builder.Configuration["Cors:ClientUrl"];

        if (!string.IsNullOrWhiteSpace(clientUrl))
        {
            policy.WithOrigins(clientUrl)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WoofBnB API",
        Version = "v1",
        Description = "WoofBnB Backend API Documentation",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT access token.",
    });

    // Per-operation, not global — only [Authorize] actions (e.g. GET /auth/me) should
    // show the padlock, matching server/docs/swagger.js's per-route bearerAuth security.
    options.OperationFilter<AuthorizeOperationFilter>();
});

// The SQL check is tagged "ready" so liveness and readiness can be probed separately
// (see MapHealthChecks below). Untagged checks would run on every endpoint, which is
// exactly what makes a database blip look like a dead process to an orchestrator.
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, tags: ["ready"]);

var app = builder.Build();

// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Node never redirects to HTTPS in any environment (server/src/server.js listens plain
// HTTP); forcing a redirect here would 301 the browser's CORS preflight OPTIONS request
// and break cross-origin calls from the Vite dev server. Deliberately omitted.
//
// HTTPS/HSTS and forwarded-header handling are intentionally still absent: the correct
// configuration depends on the deployment topology (TLS terminated upstream vs. at
// Kestrel), which is undecided. Do not add either without that decision.

// First in the pipeline so the headers are applied to every response, including those
// written by ExceptionHandlingMiddleware and the JWT challenge handlers.
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Correlates every log line for a single request, and surfaces the same id to the client
// so a user-reported failure can be traced to its logs. Placed after the exception handler
// so the scope is still open while an exception is being converted to a response.
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors(ClientCorsPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Unchanged, and still the aggregate check — existing probes and HealthEndpointTests
// depend on this exact path and its plain-text "Healthy" body.
app.MapHealthChecks("/health");

// Liveness: is the process up and able to respond at all? Runs no checks, so a database
// outage can never cause an orchestrator to kill an otherwise-healthy instance.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
});

// Readiness: should this instance receive traffic? Runs the "ready"-tagged SQL check.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.Run();
