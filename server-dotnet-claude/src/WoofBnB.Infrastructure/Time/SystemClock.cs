using WoofBnB.Application.Common.Abstractions;

namespace WoofBnB.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
