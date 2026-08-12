using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using WoofBnB.Application.Common.Responses;
using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Domain.Entities;
using WoofBnB.Domain.ValueObjects;
using WoofBnB.Infrastructure.Persistence;

namespace WoofBnB.IntegrationTests;

/// <summary>
/// End-to-end tests against the real HTTP pipeline (WoofBnBApiFactory) and a real
/// LocalDB database — mirrors server/src/modules/petsitter/**'s observable behaviour.
/// None of these requests carry an Authorization header, matching Node's petsitter
/// routes, which have no `authenticate` middleware on any of the three endpoints.
/// </summary>
public class PetSitterEndpointsTests : IAsyncLifetime
{
    private readonly WoofBnBApiFactory _factory = new();
    private readonly List<Guid> _createdIds = [];
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

        foreach (var id in _createdIds)
        {
            var petSitter = await db.PetSitters.FindAsync(id);

            if (petSitter is not null)
            {
                db.PetSitters.Remove(petSitter);
            }
        }

        await db.SaveChangesAsync();

        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<PetSitter> SeedPetSitterAsync(
        string email,
        double longitude,
        double latitude,
        DateTime? createdAt = null,
        string[]? amenities = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();

        var now = createdAt ?? DateTime.UtcNow;

        var petSitter = new PetSitter
        {
            Id = Guid.CreateVersion7(),
            Name = "Seeded Sitter",
            Email = email,
            Phone = "9876543210",
            Bio = "Seeded integration test fixture pet sitter with a long enough bio.",
            Address = "Connaught Place, New Delhi",
            Location = new Point(longitude, latitude) { SRID = 4326 },
            WorkingHours = new WorkingHours { Start = "09:00", End = "18:00" },
            Amenities = (amenities ?? ["Dog Walking"])
                .Select((a, i) => new PetSitterAmenity { Amenity = a, SortOrder = i })
                .ToList(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PetSitters.Add(petSitter);
        await db.SaveChangesAsync();
        _createdIds.Add(petSitter.Id);

        return petSitter;
    }

    private static object ValidCreatePayload(string email, double[] coordinates, string[]? amenities = null) => new
    {
        name = "John Doe",
        email,
        phone = "9876543210",
        bio = "Professional pet sitter with 5 years of experience.",
        address = "Connaught Place, New Delhi",
        location = new { type = "Point", coordinates },
        workingHours = new { start = "09:00", end = "18:00" },
        amenities = amenities ?? ["Dog Walking", "Indoor Stay"],
        profileImage = "",
    };

    // ---------------------------------------------------------------- Create

    [Fact]
    public async Task Create_ValidRequest_Returns201WithNodeCompatibleEnvelopeAndNestedShape()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/petsitters", ValidCreatePayload("create-success@example.com", [77.209, 28.6139]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PetSitterDto>>();
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.Equal(201, body.StatusCode);
        Assert.Equal("Pet sitter registered successfully", body.Message);

        var data = body.Data!;
        _createdIds.Add(Guid.Parse(data.Id));

        Assert.Equal("Point", data.Location.Type);
        Assert.Equal([77.209, 28.6139], data.Location.Coordinates);
        Assert.Equal("09:00", data.WorkingHours.Start);
        Assert.Equal("18:00", data.WorkingHours.End);
        Assert.Equal(["Dog Walking", "Indoor Stay"], data.Amenities);
        Assert.Equal("", data.ProfileImage);
    }

    [Fact]
    public async Task Create_PersistsToTheDatabase_WithGeographyLocation()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/petsitters", ValidCreatePayload("create-persist@example.com", [77.209, 28.6139]));

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PetSitterDto>>();
        var id = Guid.Parse(body!.Data!.Id);
        _createdIds.Add(id);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();
        var reloaded = await db.PetSitters.AsNoTracking().FirstAsync(p => p.Id == id);

        Assert.Equal(77.209, reloaded.Location.X, precision: 6);
        Assert.Equal(28.6139, reloaded.Location.Y, precision: 6);
    }

    [Fact]
    public async Task Create_DuplicateEmailSameCase_Returns409WithNodesExactMessage()
    {
        await SeedPetSitterAsync("duplicate-exact@example.com", 77.209, 28.6139);

        var response = await _client.PostAsJsonAsync(
            "/api/petsitters", ValidCreatePayload("duplicate-exact@example.com", [77.209, 28.6139]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("A pet sitter with this email already exists", body!.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Create_DuplicateEmailMixedCase_Returns409_NotFiveHundred()
    {
        // Decision D-8: Node's own pre-check isn't lowercased, so a mixed-case duplicate
        // slips past it and 500s at Mongo's unique index instead. This must be 409.
        await SeedPetSitterAsync("duplicate-mixedcase@example.com", 77.209, 28.6139);

        var response = await _client.PostAsJsonAsync(
            "/api/petsitters", ValidCreatePayload("Duplicate-MixedCase@Example.com", [77.209, 28.6139]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidEmail_Returns400WithFieldAndMessage()
    {
        var payload = ValidCreatePayload("not-an-email", [77.209, 28.6139]);

        var response = await _client.PostAsJsonAsync("/api/petsitters", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e => e.GetProperty("field").GetString() == "email");
    }

    [Fact]
    public async Task Create_CoordinatesOutOfRange_Returns400_NotFiveHundred()
    {
        // Decision D-5: without this validation, an out-of-range coordinate would hit
        // SQL Server's geography type, which hard-errors, surfacing as an unhandled 500.
        var payload = ValidCreatePayload("out-of-range@example.com", [200.0, 28.6139]);

        var response = await _client.PostAsJsonAsync("/api/petsitters", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "location.coordinates" &&
            e.GetProperty("message").GetString() == "Longitude must be between -180 and 180");
    }

    [Fact]
    public async Task Create_EmptyAmenitiesArray_Returns201()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/petsitters", ValidCreatePayload("empty-amenities@example.com", [77.209, 28.6139], amenities: []));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PetSitterDto>>();
        _createdIds.Add(Guid.Parse(body!.Data!.Id));
        Assert.Empty(body.Data.Amenities);
    }

    [Fact]
    public async Task Create_InvalidAmenity_Returns400WithIndexedFieldPath()
    {
        var payload = ValidCreatePayload(
            "invalid-amenity@example.com", [77.209, 28.6139], amenities: ["Dog Walking", "Not A Real Amenity"]);

        var response = await _client.PostAsJsonAsync("/api/petsitters", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "amenities.1" &&
            e.GetProperty("message").GetString() ==
                "Invalid option: expected one of \"Dog Walking\"|\"Medication\"|\"24x7 Care\"|\"Training\"|" +
                "\"Vet Nearby\"|\"Indoor Stay\"|\"Outdoor Play\"|\"CCTV\"|\"Pickup Drop\"|\"Large Yard\"|" +
                "\"Small Pets\"|\"Cats\"|\"Dogs\"|\"Birds\"");
    }

    [Fact]
    public async Task Create_WithoutAuthorizationHeader_StillSucceeds()
    {
        Assert.Null(_client.DefaultRequestHeaders.Authorization);

        var response = await _client.PostAsJsonAsync(
            "/api/petsitters", ValidCreatePayload("no-auth-create@example.com", [77.209, 28.6139]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PetSitterDto>>();
        _createdIds.Add(Guid.Parse(body!.Data!.Id));
    }

    // ---------------------------------------------------------------- GetAll

    [Fact]
    public async Task GetAll_ReturnsSeededSitters_OrderedByCreatedAtDescending()
    {
        var older = await SeedPetSitterAsync("getall-older@example.com", 77.209, 28.6139, DateTime.UtcNow.AddDays(-2));
        var newer = await SeedPetSitterAsync("getall-newer@example.com", 77.21, 28.62, DateTime.UtcNow.AddDays(-1));

        var response = await _client.GetAsync("/api/petsitters");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PetSitterDto>>>();
        Assert.Equal("Pet sitters fetched successfully", body!.Message);

        var ids = body.Data!.Select(d => Guid.Parse(d.Id)).ToList();
        var newerIndex = ids.IndexOf(newer.Id);
        var olderIndex = ids.IndexOf(older.Id);

        Assert.True(newerIndex < olderIndex, "More recently created pet sitters must appear first.");
    }

    [Fact]
    public async Task GetAll_WithoutAuthorizationHeader_StillSucceeds()
    {
        var response = await _client.GetAsync("/api/petsitters");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------------------------------------------------------------- Nearby

    [Fact]
    public async Task Nearby_OrdersResultsNearestFirst()
    {
        // Connaught Place as the search origin; India Gate (~2km) further than a point
        // ~100m away.
        var near = await SeedPetSitterAsync("nearby-near@example.com", 77.2100, 28.6140);
        var far = await SeedPetSitterAsync("nearby-far@example.com", 77.2295, 28.6129);

        var response = await _client.GetAsync("/api/petsitters/nearby?lat=28.6139&lng=77.209&radius=15000");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PetSitterDto>>>();
        var ids = body!.Data!.Select(d => Guid.Parse(d.Id)).Where(id => id == near.Id || id == far.Id).ToList();

        Assert.Equal([near.Id, far.Id], ids);
    }

    [Fact]
    public async Task Nearby_ExcludesSittersBeyondTheRadius()
    {
        var inRange = await SeedPetSitterAsync("nearby-inrange@example.com", 77.2100, 28.6140);
        var outOfRange = await SeedPetSitterAsync("nearby-outofrange@example.com", 77.2295, 28.6129); // ~2km away

        var response = await _client.GetAsync("/api/petsitters/nearby?lat=28.6139&lng=77.209&radius=500");

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PetSitterDto>>>();
        var ids = body!.Data!.Select(d => Guid.Parse(d.Id)).ToList();

        Assert.Contains(inRange.Id, ids);
        Assert.DoesNotContain(outOfRange.Id, ids);
    }

    [Fact]
    public async Task Nearby_MissingRadius_DefaultsToFiveThousandMeters()
    {
        var withinDefault = await SeedPetSitterAsync("nearby-default-in@example.com", 77.2100, 28.6140); // ~100m
        var beyondDefault = await SeedPetSitterAsync("nearby-default-out@example.com", 77.35, 28.75); // >5km

        var response = await _client.GetAsync("/api/petsitters/nearby?lat=28.6139&lng=77.209");

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PetSitterDto>>>();
        var ids = body!.Data!.Select(d => Guid.Parse(d.Id)).ToList();

        Assert.Contains(withinDefault.Id, ids);
        Assert.DoesNotContain(beyondDefault.Id, ids);
    }

    [Fact]
    public async Task Nearby_NoSittersInRange_ReturnsEmptyArrayNotNull()
    {
        var response = await _client.GetAsync("/api/petsitters/nearby?lat=1.3521&lng=103.8198&radius=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var raw = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(raw);

        Assert.Equal(JsonValueKind.Array, json.RootElement.GetProperty("data").ValueKind);
        Assert.Empty(json.RootElement.GetProperty("data").EnumerateArray());
    }

    [Theory]
    [InlineData("lat=91&lng=77.209", "Latitude must be between -90 and 90")]
    [InlineData("lat=-91&lng=77.209", "Latitude must be between -90 and 90")]
    [InlineData("lat=28.6139&lng=181", "Longitude must be between -180 and 180")]
    [InlineData("lat=28.6139&lng=-181", "Longitude must be between -180 and 180")]
    [InlineData("lat=28.6139&lng=77.209&radius=0", "Radius must be greater than 0")]
    [InlineData("lat=28.6139&lng=77.209&radius=-100", "Radius must be greater than 0")]
    public async Task Nearby_InvalidQueryParameters_Returns400WithNodesExactMessage(string query, string expectedMessage)
    {
        var response = await _client.GetAsync($"/api/petsitters/nearby?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e => e.GetProperty("message").GetString() == expectedMessage);
    }

    [Fact]
    public async Task Nearby_MissingLatAndLng_Returns400WithNodesMissingKeyMessage_NotTheRangeMessage()
    {
        // Confirmed by a live differential run (parity-tests/PARITY_REPORT.md): a wholly
        // missing lat/lng gets a different message than an out-of-range one (see
        // Nearby_InvalidQueryParameters_Returns400WithNodesExactMessage above).
        var response = await _client.GetAsync("/api/petsitters/nearby");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = body!.RootElement.GetProperty("errors").EnumerateArray().ToList();

        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "lat" &&
            e.GetProperty("message").GetString() == "Invalid input: expected number, received NaN");
        Assert.Contains(errors, e =>
            e.GetProperty("field").GetString() == "lng" &&
            e.GetProperty("message").GetString() == "Invalid input: expected number, received NaN");
    }

    [Fact]
    public async Task Nearby_WithoutAuthorizationHeader_StillSucceeds()
    {
        var response = await _client.GetAsync("/api/petsitters/nearby?lat=28.6139&lng=77.209&radius=5000");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
