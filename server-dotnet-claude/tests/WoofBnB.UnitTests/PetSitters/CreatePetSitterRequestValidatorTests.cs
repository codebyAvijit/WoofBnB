using WoofBnB.Application.PetSitters.DTOs;
using WoofBnB.Application.PetSitters.Validators;

namespace WoofBnB.UnitTests.PetSitters;

/// <summary>Mirrors server/src/modules/petsitter/petsitter.validation.js:createPetSitterSchema.</summary>
public class CreatePetSitterRequestValidatorTests
{
    private readonly CreatePetSitterRequestValidator _validator = new();

    private static CreatePetSitterRequest ValidRequest() => new()
    {
        Name = "John Doe",
        Email = "john@example.com",
        Phone = "9876543210",
        Bio = "Professional pet sitter with 5 years of experience.",
        Address = "Connaught Place, New Delhi",
        Location = new LocationDto { Type = "Point", Coordinates = [77.209, 28.6139] },
        WorkingHours = new WorkingHoursDto { Start = "09:00", End = "18:00" },
        Amenities = ["Dog Walking", "Indoor Stay"],
        ProfileImage = "",
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        Assert.True(_validator.Validate(ValidRequest()).IsValid);
    }

    [Fact]
    public void Validate_EmptyAmenitiesArray_IsValid()
    {
        // server/src/modules/petsitter/petsitter.validation.js:31 — z.array(z.enum(...))
        // requires the key but an empty array is a valid value.
        var request = ValidRequest();
        request.Amenities = [];

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ProfileImageAnyNonUrlString_IsValid()
    {
        // Node has no URL format check at all (decision D-10) — any string is accepted.
        var request = ValidRequest();
        request.ProfileImage = "not-a-url-at-all";

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Theory]
    [InlineData("J")]
    [InlineData("")]
    public void Validate_NameShorterThanTwoCharacters_Fails(string name)
    {
        var request = ValidRequest();
        request.Name = name;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NameLongerThanFiftyCharacters_Fails()
    {
        var request = ValidRequest();
        request.Name = new string('a', 51);

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    [InlineData("0123456789")]
    public void Validate_InvalidPhone_FailsWithNodesExactMessage(string phone)
    {
        var request = ValidRequest();
        request.Phone = phone;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Enter a valid phone number");
    }

    [Theory]
    [InlineData("9876543210")]
    [InlineData("+919876543210")]
    public void Validate_ValidPhoneFormats_Pass(string phone)
    {
        var request = ValidRequest();
        request.Phone = phone;

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_BioShorterThanTwentyCharacters_Fails()
    {
        var request = ValidRequest();
        request.Bio = "too short";

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_BioLongerThanOneThousandCharacters_Fails()
    {
        var request = ValidRequest();
        request.Bio = new string('a', 1001);

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_AddressShorterThanFiveCharacters_Fails()
    {
        var request = ValidRequest();
        request.Address = "abcd";

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_MissingLocation_FailsOnLocationField()
    {
        var request = ValidRequest();
        request.Location = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Location");
    }

    [Fact]
    public void Validate_LocationTypeNotPoint_FailsOnLocationTypeField()
    {
        var request = ValidRequest();
        request.Location = new LocationDto { Type = "LineString", Coordinates = [77.209, 28.6139] };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "location.type" && e.ErrorMessage == "Invalid location type");
    }

    [Fact]
    public void Validate_CoordinatesWithWrongLength_FailsOnCoordinatesField()
    {
        var request = ValidRequest();
        request.Location = new LocationDto { Type = "Point", Coordinates = [77.209] };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "location.coordinates" && e.ErrorMessage == "Coordinates must contain exactly 2 numbers");
    }

    [Theory]
    [InlineData(200.0, 28.6139)]
    [InlineData(-200.0, 28.6139)]
    public void Validate_LongitudeOutOfRange_FailsWithLongitudeMessage(double longitude, double latitude)
    {
        // Decision D-5: Node never range-checks coordinates on create; this is a
        // deliberate addition because SQL Server's geography column hard-errors on an
        // out-of-range point, which would otherwise surface as an unhandled 500.
        var request = ValidRequest();
        request.Location = new LocationDto { Type = "Point", Coordinates = [longitude, latitude] };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "location.coordinates" && e.ErrorMessage == "Longitude must be between -180 and 180");
    }

    [Theory]
    [InlineData(77.209, 100.0)]
    [InlineData(77.209, -100.0)]
    public void Validate_LatitudeOutOfRange_FailsWithLatitudeMessage(double longitude, double latitude)
    {
        var request = ValidRequest();
        request.Location = new LocationDto { Type = "Point", Coordinates = [longitude, latitude] };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "location.coordinates" && e.ErrorMessage == "Latitude must be between -90 and 90");
    }

    [Fact]
    public void Validate_MissingWorkingHours_FailsOnWorkingHoursField()
    {
        var request = ValidRequest();
        request.WorkingHours = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkingHours");
    }

    [Fact]
    public void Validate_EmptyWorkingHoursStrings_AreValid()
    {
        // server/src/modules/petsitter/petsitter.validation.js:25-29 only checks
        // z.string() — an empty string is a valid (if useless) value, not a required-ness
        // failure, since Zod's plain z.string() has no .min(1).
        var request = ValidRequest();
        request.WorkingHours = new WorkingHoursDto { Start = "", End = "" };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_MissingAmenitiesKey_FailsOnAmenitiesField()
    {
        var request = ValidRequest();
        request.Amenities = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amenities");
    }

    [Fact]
    public void Validate_UnknownAmenity_FailsWithNodesExactMessage_AndIndexedFieldPath()
    {
        var request = ValidRequest();
        request.Amenities = ["Dog Walking", "Not A Real Amenity"];

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "Amenities[1]" && e.ErrorMessage == "Invalid pet sitter amenity");
    }
}
