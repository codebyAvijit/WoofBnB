using WoofBnB.Application.Common.Abstractions;

namespace WoofBnB.UnitTests.TestSupport;

public sealed class FakeClock : IClock
{
    public FakeClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}
