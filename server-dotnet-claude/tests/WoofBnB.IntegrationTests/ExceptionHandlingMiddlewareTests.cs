using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WoofBnB.Api.Middleware;
using WoofBnB.Api.Serialization;
using WoofBnB.Application.Common.Abstractions;
using WoofBnB.Application.Common.Exceptions;

namespace WoofBnB.IntegrationTests;

/// <summary>
/// Exercises ExceptionHandlingMiddleware in a real ASP.NET pipeline (via TestServer)
/// without touching the production Program.cs — the terminal middleware here stands in
/// for "some controller action threw", mirroring server/src/middlewares/error.middleware.js's
/// two cases: a controlled AppException, and an unexpected exception.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(HttpStatusCode StatusCode, string Body)> RunAsync(
        string environmentName,
        RequestDelegate terminalMiddleware)
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .UseEnvironment(environmentName)
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddSingleton<IClock>(new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
                        services.AddSingleton(WoofBnBJsonOptions.CreateDefault());
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<ExceptionHandlingMiddleware>();
                        app.Run(terminalMiddleware);
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        return (response.StatusCode, body);
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime utcNow) => UtcNow = utcNow;

        public DateTime UtcNow { get; }
    }

    [Fact]
    public async Task AppException_MapsToItsOwnStatusCodeMessageAndErrorCode()
    {
        var (statusCode, body) = await RunAsync(
            "Production",
            _ => throw AppException.Conflict("A pet sitter with this email already exists"));

        Assert.Equal(HttpStatusCode.Conflict, statusCode);

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal(409, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("A pet sitter with this email already exists", root.GetProperty("message").GetString());
        Assert.Equal("CONFLICT", root.GetProperty("errorCode").GetString());
        Assert.False(root.TryGetProperty("stack", out _));
    }

    [Fact]
    public async Task UnhandledException_MapsTo500WithGenericMessage_NeverLeakingRawExceptionText()
    {
        var (statusCode, body) = await RunAsync(
            "Production",
            _ => throw new InvalidOperationException("connection string: Server=prod-db;Password=super-secret"));

        Assert.Equal(HttpStatusCode.InternalServerError, statusCode);

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal("An unexpected error occurred.", root.GetProperty("message").GetString());
        Assert.Equal("INTERNAL_SERVER_ERROR", root.GetProperty("errorCode").GetString());
        Assert.DoesNotContain("super-secret", body);
        Assert.False(root.TryGetProperty("stack", out _));
    }

    [Fact]
    public async Task UnhandledException_InDevelopment_IncludesStackTrace()
    {
        var (statusCode, body) = await RunAsync(
            "Development",
            _ => throw new InvalidOperationException("boom"));

        Assert.Equal(HttpStatusCode.InternalServerError, statusCode);

        using var json = JsonDocument.Parse(body);

        Assert.True(json.RootElement.TryGetProperty("stack", out var stack));
        Assert.False(string.IsNullOrWhiteSpace(stack.GetString()));
    }
}
