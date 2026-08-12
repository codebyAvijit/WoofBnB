using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Application.PetSitters.Validators;

namespace WoofBnB.UnitTests.PetSitters;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.validation.js:nearbyPetSitterSchema.</summary>
public class NearbyPetSitterQueryValidatorTests
{
    private readonly NearbyPetSitterQueryValidator _validator = new();

    [Fact]
    public void Validate_ValidLatLngRadius_HasNoErrors()
    {
        var result = _validator.Validate(new NearbyPetSitterQuery { Lat = 28.6139, Lng = 77.209, Radius = 5000 });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MissingRadius_IsValid()
    {
        // Node's z.coerce.number().positive().default(5000) never runs .positive() when
        // the input is undefined — a missing radius always passes validation; the
        // default is applied downstream in PetSitterService, not here.
        var result = _validator.Validate(new NearbyPetSitterQuery { Lat = 28.6139, Lng = 77.209, Radius = null });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MissingLat_FailsWithZodsTypeStageMessage_NotTheRangeMessage()
    {
        // Confirmed by a live differential run (parity-tests/PARITY_REPORT.md): Zod's
        // z.coerce.number() fails a MISSING lat at the type/coercion stage with its own
        // default message, before .min()/.max() ever run — it is NOT the same message an
        // out-of-range value gets.
        var result = _validator.Validate(new NearbyPetSitterQuery { Lat = null, Lng = 77.209 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Lat" && e.ErrorMessage == "Invalid input: expected number, received NaN");
    }

    [Theory]
    [InlineData(91.0)]
    [InlineData(-91.0)]
    public void Validate_LatitudeOutOfRange_FailsWithNodesExactMessage(double lat)
    {
        var result = _validator.Validate(new NearbyPetSitterQuery { Lat = lat, Lng = 77.209 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Lat" && e.ErrorMessage == "Latitude must be between -90 and 90");
    }

    [Fact]
    public void Validate_MissingLng_FailsWithZodsTypeStageMessage_NotTheRangeMessage()
    {
        var result = _validator.Validate(new NearbyPetSitterQuery { Lat = 28.6139, Lng = null });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Lng" && e.ErrorMessage == "Invalid input: expected number, received NaN");
    }

    [Theory]
    [InlineData(181.0)]
    [InlineData(-181.0)]
    public void Validate_LongitudeOutOfRange_FailsWithNodesExactMessage(double lng)
    {
        var result = _validator.Validate(new NearbyPetSitterQuery { Lat = 28.6139, Lng = lng });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Lng" && e.ErrorMessage == "Longitude must be between -180 and 180");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-100.0)]
    public void Validate_NonPositiveRadius_FailsWithNodesExactMessage(double radius)
    {
        var result = _validator.Validate(new NearbyPetSitterQuery { Lat = 28.6139, Lng = 77.209, Radius = radius });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Radius" && e.ErrorMessage == "Radius must be greater than 0");
    }
}
