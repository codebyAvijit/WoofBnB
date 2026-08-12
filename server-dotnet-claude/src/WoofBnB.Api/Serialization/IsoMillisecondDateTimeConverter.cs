using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoofBnB.Api.Serialization;

/// <summary>
/// Node emits timestamps via `new Date().toISOString()`, which always produces exactly
/// 3 fractional-second digits and a trailing "Z" (e.g. "2026-07-31T09:30:00.000Z").
/// The default System.Text.Json DateTime format can emit up to 7 fractional digits and
/// omits "Z" for Unspecified/Local kinds, so every value handled by this converter is
/// treated as UTC and written in Node's exact format for contract parity.
/// </summary>
public sealed class IsoMillisecondDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new JsonException("Expected a non-null ISO-8601 date-time string.");

        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();

        writer.WriteStringValue(utcValue.ToString(Format, CultureInfo.InvariantCulture));
    }
}
