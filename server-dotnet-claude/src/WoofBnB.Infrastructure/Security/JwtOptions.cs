namespace WoofBnB.Infrastructure.Security;

/// <summary>
/// Maps to server/.env.example's JWT_SECRET / JWT_EXPIRES_IN. Node's "1d" duration string
/// has no direct .NET equivalent, so it is expressed here as a plain integer number of
/// minutes (1440 = "1d") — a config-format translation, not a behavioral change.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Secret { get; set; }

    public required int ExpiresInMinutes { get; set; }
}
