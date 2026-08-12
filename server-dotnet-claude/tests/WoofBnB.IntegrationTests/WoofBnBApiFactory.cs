using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WoofBnB.IntegrationTests;

/// <summary>
/// Boots the real Program.cs pipeline (unmodified) against appsettings.Development.json,
/// so tests exercise the exact CORS/Swagger/health configuration that ships to developers,
/// matching how the Node server is actually run locally.
/// </summary>
public sealed class WoofBnBApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
