using NSubstitute;
using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Application.PetSitters;
using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Domain.Entities;
using WoofBnB.UnitTests.TestSupport;

namespace WoofBnB.UnitTests.PetSitters;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.service.js.</summary>
public class PetSitterServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc);

    private readonly IPetSitterRepository _repository = Substitute.For<IPetSitterRepository>();
    private readonly PetSitterService _service;

    public PetSitterServiceTests()
    {
        _service = new PetSitterService(_repository, new FakeClock(FixedNow));

        // Echo back whatever entity was passed to CreateAsync, like a real save would.
        _repository.CreateAsync(Arg.Any<PetSitter>()).Returns(callInfo => callInfo.Arg<PetSitter>());
    }

    private static CreatePetSitterRequest ValidRequest() => new()
    {
        Name = "John Doe",
        Email = "John@Example.com",
        Phone = "9876543210",
        Bio = "Professional pet sitter with 5 years of experience.",
        Address = "Connaught Place, New Delhi",
        Location = new LocationDto { Type = "Point", Coordinates = [77.209, 28.6139] },
        WorkingHours = new WorkingHoursDto { Start = "09:00", End = "18:00" },
        Amenities = ["Indoor Stay", "Dog Walking"],
        ProfileImage = "",
    };

    [Fact]
    public async Task RegisterAsync_NormalizesEmailToLowercase_BeforeDuplicateCheckAndOnTheStoredEntity()
    {
        _repository.GetByEmailAsync("john@example.com").Returns((PetSitter?)null);

        var dto = await _service.RegisterAsync(ValidRequest());

        await _repository.Received(1).GetByEmailAsync("john@example.com");
        Assert.Equal("john@example.com", dto.Email);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_PreservesAmenityOrderAsSortOrder()
    {
        _repository.GetByEmailAsync(Arg.Any<string>()).Returns((PetSitter?)null);

        PetSitter? captured = null;
        await _repository.CreateAsync(Arg.Do<PetSitter>(p => captured = p));

        await _service.RegisterAsync(ValidRequest());

        Assert.NotNull(captured);
        Assert.Equal(["Indoor Stay", "Dog Walking"], captured!.Amenities.OrderBy(a => a.SortOrder).Select(a => a.Amenity));
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_MapsLocationAsLongitudeXLatitudeY()
    {
        _repository.GetByEmailAsync(Arg.Any<string>()).Returns((PetSitter?)null);

        var dto = await _service.RegisterAsync(ValidRequest());

        Assert.Equal([77.209, 28.6139], dto.Location.Coordinates);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_SetsCreatedAtAndUpdatedAt_FromInjectedClock()
    {
        _repository.GetByEmailAsync(Arg.Any<string>()).Returns((PetSitter?)null);

        var dto = await _service.RegisterAsync(ValidRequest());

        Assert.Equal(FixedNow, dto.CreatedAt);
        Assert.Equal(FixedNow, dto.UpdatedAt);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict_WithNodesExactMessage()
    {
        _repository.GetByEmailAsync(Arg.Any<string>()).Returns(new PetSitter
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Email = "john@example.com",
            Phone = "9876543210",
            Bio = "Existing bio that is long enough to pass validation checks.",
            Address = "Somewhere",
            Location = new NetTopologySuite.Geometries.Point(77.209, 28.6139) { SRID = 4326 },
            WorkingHours = new WoofBnB.Domain.ValueObjects.WorkingHours { Start = "09:00", End = "18:00" },
        });

        var exception = await Assert.ThrowsAsync<AppException>(() => _service.RegisterAsync(ValidRequest()));

        Assert.Equal(409, exception.StatusCode);
        Assert.Equal("A pet sitter with this email already exists", exception.Message);
        await _repository.DidNotReceive().CreateAsync(Arg.Any<PetSitter>());
    }

    [Fact]
    public async Task GetAllAsync_DelegatesToRepositoryAndMapsResults()
    {
        _repository.GetAllAsync().Returns([]);

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
        await _repository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task GetNearbyAsync_MissingRadius_AppliesFiveThousandMeterDefault()
    {
        _repository.GetNearbyAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<double>()).Returns([]);

        await _service.GetNearbyAsync(new NearbyPetSitterQuery { Lat = 28.6139, Lng = 77.209, Radius = null });

        await _repository.Received(1).GetNearbyAsync(28.6139, 77.209, 5000);
    }

    [Fact]
    public async Task GetNearbyAsync_ExplicitRadius_IsPassedThroughUnchanged()
    {
        _repository.GetNearbyAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<double>()).Returns([]);

        await _service.GetNearbyAsync(new NearbyPetSitterQuery { Lat = 28.6139, Lng = 77.209, Radius = 15000 });

        await _repository.Received(1).GetNearbyAsync(28.6139, 77.209, 15000);
    }
}
