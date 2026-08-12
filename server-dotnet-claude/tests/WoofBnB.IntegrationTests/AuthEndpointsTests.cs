using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WoofBnB.Application.Auth.DTOs;
using WoofBnB.Application.Common.Responses;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;
using WoofBnB.Infrastructure.Persistence;
using WoofBnB.Infrastructure.Security;

namespace WoofBnB.IntegrationTests;

/// <summary>
/// End-to-end tests against the real HTTP pipeline (WoofBnBApiFactory/AuthTestApiFactory)
/// and a real LocalDB database — mirrors server/src/modules/auth/**'s observable
/// behaviour, verified request-by-request rather than unit-by-unit.
/// </summary>
public class AuthEndpointsTests : IAsyncLifetime
{
    private readonly AuthTestApiFactory _factory = new();
    private readonly List<Guid> _createdUserIds = [];
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();

        foreach (var id in _createdUserIds)
        {
            var user = await db.Users.FindAsync(id);

            if (user is not null)
            {
                db.Users.Remove(user);
            }
        }

        await db.SaveChangesAsync();

        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<User> SeedUserAsync(
        string email,
        string plainPassword,
        bool isActive = true,
        Guid? id = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();
        var hasher = new BCryptPasswordHasher(10);

        var user = new User
        {
            Id = id ?? Guid.CreateVersion7(),
            Name = "Test Admin",
            Email = email,
            PasswordHash = hasher.Hash(plainPassword),
            Role = UserRoles.Admin,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        _createdUserIds.Add(user.Id);

        return user;
    }

    private string MintToken(Guid userId, string role = "admin", int expiresInMinutes = 1440)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(AuthTestApiFactory.JwtSecret));
        var now = DateTime.UtcNow;

        var payload = new JwtPayload
        {
            ["id"] = userId.ToString(),
            ["role"] = role,
            ["iat"] = EpochTime.GetIntDate(now),
            ["exp"] = EpochTime.GetIntDate(now.AddMinutes(expiresInMinutes)),
        };

        var token = new JwtSecurityToken(
            new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256)),
            payload);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ---------------------------------------------------------------- Login

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithUserAndAccessToken()
    {
        var user = await SeedUserAsync("login-success@example.com", "password123");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = user.Email, password = "password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResultDto>>();

        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.Equal("Login successful", body.Message);
        Assert.NotNull(body.Data);
        Assert.Equal(user.Id.ToString(), body.Data!.User.Id);
        Assert.False(string.IsNullOrWhiteSpace(body.Data.AccessToken));
        Assert.NotNull(body.Data.User.LastLogin);
    }

    [Fact]
    public async Task Login_PersistsLastLoginToTheDatabase()
    {
        var user = await SeedUserAsync("login-lastlogin@example.com", "password123");

        await _client.PostAsJsonAsync("/api/auth/login", new { email = user.Email, password = "password123" });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();
        var reloaded = await db.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);

        Assert.NotNull(reloaded.LastLogin);
    }

    [Fact]
    public async Task Login_MixedCaseEmail_StillSucceeds()
    {
        // Decision D-7: Node's own login lookup is case-sensitive against lowercased
        // storage and would 401 here — fixed in this implementation.
        var user = await SeedUserAsync("mixedcase@example.com", "password123");

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = "MixedCase@Example.com", password = "password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized_WithNodesExactMessage()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = "nobody@example.com", password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("Invalid email or password", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized_WithNodesExactMessage()
    {
        var user = await SeedUserAsync("wrongpassword@example.com", "correct-password");

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = user.Email, password = "incorrect-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("Invalid email or password", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Login_DisabledAccount_ReturnsForbidden_EvenWithCorrectPassword()
    {
        var user = await SeedUserAsync("disabled-login@example.com", "password123", isActive: false);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = user.Email, password = "password123" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("Your account has been disabled", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Login_InvalidEmailFormat_ReturnsValidationError_OnEmailField()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = "not-an-email", password = "password123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "email" &&
            e.GetProperty("message").GetString() == "Please provide a valid email address");
    }

    [Fact]
    public async Task Login_PasswordTooShort_ReturnsValidationError_OnPasswordField()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = "admin@example.com", password = "short1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "password" &&
            e.GetProperty("message").GetString() == "Password must be at least 8 characters");
    }

    [Fact]
    public async Task Login_EmptyBody_ReturnsValidationError_WithNodesMissingKeyMessage_OnBothFields()
    {
        // Confirmed by a live differential run (parity-tests/PARITY_REPORT.md): a wholly
        // missing email/password key gets a different message than a present-but-invalid
        // one (see Login_InvalidEmailFormat_ReturnsValidationError_OnEmailField above) —
        // this exercises that distinction through the real model binder, not just the
        // validator class directly.
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "email" &&
            e.GetProperty("message").GetString() == "Invalid input: expected string, received undefined");
        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "password" &&
            e.GetProperty("message").GetString() == "Invalid input: expected string, received undefined");
    }

    // ---------------------------------------------------------------- GetCurrentUser (/me)

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUser()
    {
        var user = await SeedUserAsync("me-success@example.com", "password123");
        var token = MintToken(user.Id);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.Equal(user.Id.ToString(), body!.Data!.Id);
        Assert.Equal(user.Email, body.Data.Email);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized_AuthenticationRequired()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("Authentication required", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task GetCurrentUser_WithMalformedToken_ReturnsUnauthorized_InvalidOrExpiredToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-jwt");
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("Invalid or expired token", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task GetCurrentUser_WithExpiredToken_ReturnsUnauthorized_InvalidOrExpiredToken()
    {
        var user = await SeedUserAsync("me-expired@example.com", "password123");

        // Already expired the instant it's minted: exp = now - 1 minute.
        var expiredToken = MintToken(user.Id, expiresInMinutes: -1);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("Invalid or expired token", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task GetCurrentUser_ForNonExistentUser_ReturnsUnauthorized_UserNotFound()
    {
        // A structurally valid, correctly-signed token whose id claim matches no row in
        // the database — e.g. a since-deleted account. Mirrors
        // server/src/middlewares/auth.middleware.js:21-23's own 401 "User not found".
        var token = MintToken(Guid.NewGuid());

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("User not found", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task GetCurrentUser_ForDisabledUser_ReturnsForbidden()
    {
        var user = await SeedUserAsync("me-disabled@example.com", "password123", isActive: false);
        var token = MintToken(user.Id);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("Your account has been disabled", body!.RootElement.GetProperty("message").GetString());
    }

    // ---------------------------------------------------------------- Node cross-compatibility

    [Fact]
    public async Task GetCurrentUser_WithTokenSignedByRealNodeJsonwebtoken_IsAccepted()
    {
        // Generated by running, from server/ (using the real npm jsonwebtoken package):
        //   node -e "require('jsonwebtoken').sign(
        //     { id: '33333333-3333-3333-3333-333333333333', role: 'admin' },
        //     'integration-test-secret-at-least-32-bytes-long-for-hs256!!',
        //     { expiresIn: '100y' })"
        // (a 100-year expiry so this stays a durable regression check rather than a
        // token that quietly starts failing for the wrong reason months from now).
        // This proves the crypto/claims mechanics — HS256, the shared secret, and the
        // exact { id, role } claim shape — are genuinely cross-compatible: a token Node
        // itself produced is accepted end-to-end by the ASP.NET pipeline, not just
        // "should work by inspection".
        //
        // IMPORTANT CAVEAT (see PHASE STATUS report): this does NOT mean an actual
        // pre-existing Node session survives the cutover. Real Node tokens carry a
        // MongoDB ObjectId in `id`; this test's token instead carries the GUID of a
        // user seeded directly into SQL Server, because after the ID-format change
        // (decision D-1) no ObjectId can ever match a row in the new database. That is
        // an inherent, expected consequence of switching key formats, not a defect —
        // any admin with an active session at cutover time logs in again once.
        const string nodeSignedToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjMzMzMzMzMzLTMzMzMtMzMzMy0zMzMzLTMzMzMzMzMzMzMzMyIsInJvbGUiOiJhZG1pbiIsImlhdCI6MTc4NjUyOTIwOSwiZXhwIjo0OTQyMjg5MjA5fQ.8td4MgLsAktZf-5tvp8WK6AURJQ8-Xwq9G1-QpiO9SE";

        var fixedUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        await SeedUserAsync("node-cross-compat@example.com", "password123", id: fixedUserId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", nodeSignedToken);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.Equal(fixedUserId.ToString(), body!.Data!.Id);
    }
}
