using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using WoofBnB.Api.Authorization;
using WoofBnB.Api.Filters;
using WoofBnB.Api.Middleware;
using WoofBnB.Api.Serialization;
using WoofBnB.Api.Swagger;
using WoofBnB.Application.Auth;
using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Responses;
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

builder.Services.AddDbContext<WoofBnBDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.UseNetTopologySuite()));

// ---- Authentication (mirrors server/src/modules/auth/**) ----

// Cheap presence check only — fails fast on a deployment that never configured a
// secret at all. Deliberately NOT used to build TokenValidationParameters below: that
// must read JwtOptions lazily through DI (see AddOptions<JwtBearerOptions>().Configure
// below), because a value captured here, in a local variable, is frozen at this point
// in startup — before WebApplicationFactory-based tests get a chance to layer their own
// config override on top, which silently made every integration test validate against
// the wrong secret until this was caught by a failing test.
if (string.IsNullOrWhiteSpace(builder.Configuration[$"{JwtOptions.SectionName}:Secret"]))
{
    throw new InvalidOperationException("Missing required configuration: Jwt:Secret.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
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

builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

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
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(ClientCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
