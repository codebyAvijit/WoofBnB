namespace WoofBnB.Application.Common.Abstractions;

/// <summary>
/// Abstracts "now" so response timestamps and entity CreatedAt/UpdatedAt values are
/// deterministic in tests, instead of calling DateTime.UtcNow directly.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
