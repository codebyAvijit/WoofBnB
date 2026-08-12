namespace WoofBnB.IntegrationTests;

/// <summary>
/// Node's cors() middleware (server/src/app.js:22-27) allows only CLIENT_URL, with
/// credentials. This asserts the ASP.NET named policy (Cors:ClientUrl in
/// appsettings.Development.json = http://localhost:5173, matching the Vite dev server)
/// reproduces that: the configured origin is allowed with credentials, and an
/// unconfigured origin gets no CORS headers at all — never a broad wildcard.
/// </summary>
public class CorsTests : IClassFixture<WoofBnBApiFactory>
{
    private readonly HttpClient _client;

    public CorsTests(WoofBnBApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Preflight_FromConfiguredClientOrigin_IsAllowedWithCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/health");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowedOrigins));
        Assert.Equal("http://localhost:5173", allowedOrigins!.Single());

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Credentials", out var credentials));
        Assert.Equal("true", credentials!.Single());
    }

    [Fact]
    public async Task Preflight_FromUnconfiguredOrigin_HasNoCorsHeaders()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/health");
        request.Headers.Add("Origin", "http://evil.example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
