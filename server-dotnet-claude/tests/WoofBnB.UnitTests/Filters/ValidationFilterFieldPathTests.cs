using WoofBnB.Api.Filters;

namespace WoofBnB.UnitTests.Filters;

/// <summary>
/// FluentValidation reports PascalCase.Dot.Paths; Zod's issue.path.join(".") is camelCase.
/// These assert the lowercase-first-letter-per-segment transform used to match Node's
/// { field, message } shape (server/src/middlewares/validate.middleware.js).
/// </summary>
public class ValidationFilterFieldPathTests
{
    [Theory]
    [InlineData("Email", "email")]
    [InlineData("Location.Coordinates", "location.coordinates")]
    [InlineData("WorkingHours.Start", "workingHours.start")]
    [InlineData("Amenities[0]", "amenities.0")]
    [InlineData("Amenities[12]", "amenities.12")]
    [InlineData("", "")]
    public void ToFieldPath_LowercasesFirstLetterOfEachSegment(string propertyName, string expected)
    {
        var result = ValidationFilter.ToFieldPath(propertyName);

        Assert.Equal(expected, result);
    }
}
