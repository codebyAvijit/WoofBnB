using System.Text.Json;
using WoofBnB.Api.Serialization;

namespace WoofBnB.UnitTests.Serialization;

public class IsoMillisecondDateTimeConverterTests
{
    private static readonly JsonSerializerOptions Options = WoofBnBJsonOptions.CreateDefault();

    [Fact]
    public void Write_ProducesExactlyThreeFractionalDigitsAndTrailingZ()
    {
        var value = new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(value, Options);

        Assert.Equal("\"2026-07-31T09:30:00.000Z\"", json);
    }

    [Fact]
    public void Write_ConvertsNonUtcKindToUtcBeforeFormatting()
    {
        // Node's new Date().toISOString() always renders in UTC regardless of local time.
        var localValue = DateTime.SpecifyKind(new DateTime(2026, 7, 31, 9, 30, 0), DateTimeKind.Local);

        var json = JsonSerializer.Serialize(localValue, Options);

        Assert.EndsWith("Z\"", json);
        Assert.Matches("^\"\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}Z\"$", json);
    }

    [Fact]
    public void Write_TruncatesSubMillisecondPrecisionToThreeDigits()
    {
        var value = new DateTime(2026, 7, 31, 9, 30, 0, 123, DateTimeKind.Utc).AddTicks(4567);

        var json = JsonSerializer.Serialize(value, Options);

        Assert.Equal("\"2026-07-31T09:30:00.123Z\"", json);
    }

    [Fact]
    public void Read_ParsesIsoStringBackToUtcDateTime()
    {
        var json = "\"2026-07-31T09:30:00.000Z\"";

        var value = JsonSerializer.Deserialize<DateTime>(json, Options);

        Assert.Equal(new DateTime(2026, 7, 31, 9, 30, 0, DateTimeKind.Utc), value);
        Assert.Equal(DateTimeKind.Utc, value.Kind);
    }

    [Fact]
    public void NullableDateTime_RoundTripsNullWithoutACustomConverter()
    {
        DateTime? value = null;

        var json = JsonSerializer.Serialize(value, Options);

        Assert.Equal("null", json);
    }
}
