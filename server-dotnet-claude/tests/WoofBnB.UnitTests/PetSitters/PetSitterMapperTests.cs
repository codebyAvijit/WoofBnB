using NetTopologySuite.Geometries;
using WoofBnB.Application.PetSitters.Mappers;
using WoofBnB.Domain.Entities;
using WoofBnB.Domain.ValueObjects;

namespace WoofBnB.UnitTests.PetSitters;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.mapper.js:toPetSitterDto.</summary>
public class PetSitterMapperTests
{
    private static PetSitter NewPetSitter() => new()
    {
        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Name = "John Doe",
        Email = "john@example.com",
        Phone = "9876543210",
        Bio = "Professional pet sitter with 5 years of experience.",
        Address = "Connaught Place, New Delhi",
        Location = new Point(77.209, 28.6139) { SRID = 4326 },
        WorkingHours = new WorkingHours { Start = "09:00", End = "18:00" },
        Amenities =
        [
            new PetSitterAmenity { Amenity = "Indoor Stay", SortOrder = 1 },
            new PetSitterAmenity { Amenity = "Dog Walking", SortOrder = 0 },
        ],
        ProfileImage = null,
        CreatedAt = new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc),
    };

    [Fact]
    public void ToDto_MapsScalarFields()
    {
        var dto = PetSitterMapper.ToDto(NewPetSitter());

        Assert.Equal("44444444-4444-4444-4444-444444444444", dto.Id);
        Assert.Equal("John Doe", dto.Name);
        Assert.Equal("john@example.com", dto.Email);
        Assert.Equal("9876543210", dto.Phone);
        Assert.Null(dto.ProfileImage);
    }

    [Fact]
    public void ToDto_LocationCoordinates_AreOrderedLongitudeThenLatitude()
    {
        // Decision D-4: Point.X is longitude, Point.Y is latitude — the response must
        // echo [longitude, latitude], matching Node's GeoJSON coordinate order exactly.
        var dto = PetSitterMapper.ToDto(NewPetSitter());

        Assert.Equal("Point", dto.Location.Type);
        Assert.Equal([77.209, 28.6139], dto.Location.Coordinates);
    }

    [Fact]
    public void ToDto_WorkingHours_MapsStartAndEnd()
    {
        var dto = PetSitterMapper.ToDto(NewPetSitter());

        Assert.Equal("09:00", dto.WorkingHours.Start);
        Assert.Equal("18:00", dto.WorkingHours.End);
    }

    [Fact]
    public void ToDto_Amenities_ArePreservedInSortOrder_NotInsertionOrder()
    {
        var dto = PetSitterMapper.ToDto(NewPetSitter());

        Assert.Equal(["Dog Walking", "Indoor Stay"], dto.Amenities);
    }

    [Fact]
    public void ToDto_EmptyAmenities_MapsToEmptyListNotNull()
    {
        var petSitter = NewPetSitter();
        petSitter.Amenities = [];

        var dto = PetSitterMapper.ToDto(petSitter);

        Assert.NotNull(dto.Amenities);
        Assert.Empty(dto.Amenities);
    }
}
