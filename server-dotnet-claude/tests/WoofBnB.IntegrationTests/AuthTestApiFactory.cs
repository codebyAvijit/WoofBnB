using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace WoofBnB.IntegrationTests;

/// <summary>
/// A known, fixed JWT secret (unlike WoofBnBApiFactory, which uses whatever real secret
/// is configured via user-secrets) so tests can mint tokens externally — including via
/// Node's own jsonwebtoken library — and know the running app will accept them if the
/// claims/algorithm are otherwise valid. Never use this secret outside tests.
/// </summary>
public sealed class AuthTestApiFactory : WebApplicationFactory<Program>
{
    public const string JwtSecret = "integration-test-secret-at-least-32-bytes-long-for-hs256!!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = JwtSecret,
            });
        });
    }
}
