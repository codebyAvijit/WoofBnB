using System.Net;

namespace WoofBnB.IntegrationTests;

/// <summary>
/// Verifies /api/health reproduces server/src/app.js:33-38 byte-for-byte: a bare
/// { success, message } object, NOT the ApiResponse&lt;T&gt; envelope (no statusCode,
/// data, or timestamp). /health is the separate ASP.NET liveness probe CLAUDE.md §18
/// asks for, returning plain "Healthy" text.
/// </summary>
public class HealthEndpointTests : IClassFixture<WoofBnBApiFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WoofBnBApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetApiHealth_ReturnsExactNodeCompatibleBareShape()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("{\"success\":true,\"message\":\"WoofBnB API is running\"}", body);
    }

    [Fact]
    public async Task GetHealth_ReturnsPlainTextHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("text/plain", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", body);
    }
}
