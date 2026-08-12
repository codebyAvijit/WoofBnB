using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using WoofBnB.Domain.Constants;
using WoofBnB.Domain.Entities;
using WoofBnB.Domain.ValueObjects;
using WoofBnB.Infrastructure.Persistence;

namespace WoofBnB.IntegrationTests.Persistence;

/// <summary>
/// Validates the single highest-risk assumption in decision D-4 (audit §10.5/R4): that
/// NetTopologySuite's Point(x, y) maps to SQL Server's geography column with x=longitude,
/// y=latitude preserved exactly, and that STDistance returns metres comparable to Node's
/// $maxDistance. Runs against the real LocalDB database (see appsettings.Development.json)
/// through the app's own DI-configured WoofBnBDbContext — not a mock — because an x/y swap
/// is exactly the kind of bug a mock would hide. Requires the InitialCreate migration to
/// already be applied (dotnet ef database update).
/// </summary>
public class GeographyRoundTripTests : IClassFixture<WoofBnBApiFactory>, IAsyncLifetime
{
    private readonly WoofBnBApiFactory _factory;
    private readonly List<Guid> _createdIds = [];

    public GeographyRoundTripTests(WoofBnBApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();

        foreach (var id in _createdIds)
        {
            var entity = await db.PetSitters.FindAsync(id);

            if (entity is not null)
            {
                db.PetSitters.Remove(entity);
            }
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Point_RoundTripsWithXAsLongitudeAndYAsLatitude()
    {
        // Connaught Place, New Delhi — the coordinates used throughout server/docs/ examples.
        const double longitude = 77.209;
        const double latitude = 28.6139;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();

        var petSitter = NewPetSitter(longitude, latitude);
        db.PetSitters.Add(petSitter);
        await db.SaveChangesAsync();
        _createdIds.Add(petSitter.Id);

        db.ChangeTracker.Clear();

        var reloaded = await db.PetSitters.FindAsync(petSitter.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(longitude, reloaded!.Location.X, precision: 9);
        Assert.Equal(latitude, reloaded.Location.Y, precision: 9);
    }

    [Fact]
    public async Task StDistance_BetweenTwoKnownDelhiPoints_MatchesRealWorldMetres()
    {
        // Connaught Place and India Gate, New Delhi — straight-line distance is
        // well-documented as approximately 2.1-2.2 km.
        var origin = new Point(77.209, 28.6139) { SRID = 4326 };

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();

        var indiaGate = NewPetSitter(longitude: 77.2295, latitude: 28.6129);
        db.PetSitters.Add(indiaGate);
        await db.SaveChangesAsync();
        _createdIds.Add(indiaGate.Id);

        // Must run as part of an EF query so .Distance() translates to SQL Server's
        // STDistance (ellipsoidal, metres). Calling .Distance() directly on the already-
        // materialized in-memory Point instead uses NetTopologySuite's own planar
        // Euclidean distance (in the SRID's units — degrees for WGS84), which silently
        // returns a number in the wrong unit and wrong model without erroring.
        var distanceInMeters = await db.PetSitters
            .Where(p => p.Id == indiaGate.Id)
            .Select(p => p.Location.Distance(origin))
            .SingleAsync();

        // Sanity band, not an exact figure: confirms units are metres (a lng/lat-degrees
        // mixup would produce a wildly different order of magnitude) and confirms X/Y
        // weren't swapped (a swap would place the point far outside Delhi entirely).
        Assert.InRange(distanceInMeters, 1_500, 3_000);
    }

    [Fact]
    public async Task StDistance_OrdersNearestFirst_MatchingMongoNearSemantics()
    {
        var origin = new Point(77.209, 28.6139) { SRID = 4326 }; // Connaught Place

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WoofBnBDbContext>();

        var near = NewPetSitter(longitude: 77.2100, latitude: 28.6140); // ~100m away
        var far = NewPetSitter(longitude: 77.2295, latitude: 28.6129); // India Gate, ~2km away

        db.PetSitters.AddRange(near, far);
        await db.SaveChangesAsync();
        _createdIds.AddRange([near.Id, far.Id]);

        var ordered = await db.PetSitters
            .Where(p => p.Id == near.Id || p.Id == far.Id)
            .OrderBy(p => p.Location.Distance(origin))
            .Select(p => p.Id)
            .ToListAsync();

        Assert.Equal([near.Id, far.Id], ordered);
    }

    private static PetSitter NewPetSitter(double longitude, double latitude) => new()
    {
        Id = Guid.CreateVersion7(),
        Name = "Test Pet Sitter",
        Email = $"{Guid.NewGuid():N}@example.com",
        Phone = "9876543210",
        Bio = "Geography round-trip test fixture pet sitter.",
        Address = "Connaught Place, New Delhi",
        Location = new Point(longitude, latitude) { SRID = 4326 },
        WorkingHours = new WorkingHours { Start = "09:00", End = "18:00" },
        Amenities = [new PetSitterAmenity { Amenity = PetSitterAmenities.All[0], SortOrder = 0 }],
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };
}
