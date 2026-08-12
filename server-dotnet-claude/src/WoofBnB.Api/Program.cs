using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WoofBnB.Api.Filters;
using WoofBnB.Api.Middleware;
using WoofBnB.Api.Serialization;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Responses;
using WoofBnB.Infrastructure.Persistence;
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

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
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

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
