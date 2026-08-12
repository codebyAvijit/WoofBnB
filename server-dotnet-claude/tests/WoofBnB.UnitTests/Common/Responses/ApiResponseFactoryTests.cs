using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Responses;
using WoofBnB.UnitTests.TestSupport;

namespace WoofBnB.UnitTests.Common.Responses;

public class ApiResponseFactoryTests
{
    [Fact]
    public void Success_UsesInjectedClock_NotWallClockTime()
    {
        var fixedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var factory = new ApiResponseFactory(new FakeClock(fixedTime));

        var response = factory.Success(HttpStatusCodes.Ok, "Pet sitters fetched successfully", new List<string>());

        Assert.True(response.Success);
        Assert.Equal(HttpStatusCodes.Ok, response.StatusCode);
        Assert.Equal("Pet sitters fetched successfully", response.Message);
        Assert.Equal(fixedTime, response.Timestamp);
        Assert.Empty(response.Data!);
    }
}
