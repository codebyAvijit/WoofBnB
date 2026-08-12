using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;
using WoofBnB.Infrastructure.Security;
using WoofBnB.UnitTests.TestSupport;

namespace WoofBnB.UnitTests.Security;

/// <summary>
/// Asserts the generated token's payload matches server/src/modules/auth/auth.token.service.js
/// exactly: { id, role, iat, exp } and nothing else — no nbf, sub, iss, or aud, which a
/// ClaimsIdentity-based token would silently add or rewrite (audit decision R7).
/// </summary>
public class JwtTokenServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string Secret = "unit-test-secret-at-least-32-bytes-long-for-hs256!!";

    private readonly JwtTokenService _service = new(
        Options.Create(new JwtOptions { Secret = Secret, ExpiresInMinutes = 1440 }),
        new FakeClock(FixedNow));

    private static User NewUser() => new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Name = "Admin",
        Email = "admin@example.com",
        PasswordHash = "irrelevant-for-this-test",
        Role = UserRoles.Admin,
        CreatedAt = FixedNow,
        UpdatedAt = FixedNow,
    };

    [Fact]
    public void GenerateAccessToken_ContainsExactlyTheExpectedClaims_NoMoreNoLess()
    {
        var token = _service.GenerateAccessToken(NewUser());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claimTypes = jwt.Claims.Select(c => c.Type).ToHashSet();

        Assert.Equal(new HashSet<string> { "id", "role", "iat", "exp" }, claimTypes);
    }

    [Fact]
    public void GenerateAccessToken_SetsIdAndRoleClaims_ToTheUsersValues()
    {
        var user = NewUser();
        var token = _service.GenerateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == "id").Value);
        Assert.Equal(user.Role, jwt.Claims.Single(c => c.Type == "role").Value);
    }

    [Fact]
    public void GenerateAccessToken_UsesHmacSha256_MatchingJsonwebtokensDefault()
    {
        var token = _service.GenerateAccessToken(NewUser());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("HS256", jwt.Header.Alg);
    }

    [Fact]
    public void GenerateAccessToken_SetsExpiryToConfiguredMinutesFromIssuedTime()
    {
        var token = _service.GenerateAccessToken(NewUser());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(FixedNow, jwt.IssuedAt);
        Assert.Equal(FixedNow.AddMinutes(1440), jwt.ValidTo);
    }
}
