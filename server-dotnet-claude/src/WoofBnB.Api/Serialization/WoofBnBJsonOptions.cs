using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoofBnB.Api.Serialization;

/// <summary>
/// Single source of truth for JSON behaviour so MVC's JsonOptions and the standalone
/// JsonSerializerOptions used by ExceptionHandlingMiddleware never drift apart.
/// camelCase matches the Node/Zod/Mongoose property names the frontend already consumes
/// (e.g. "workingHours", "profileImage"); DefaultIgnoreCondition.Never matches Node emitting
/// explicit nulls (e.g. "profileImage": null, "errors": null) instead of omitting the key.
/// </summary>
public static class WoofBnBJsonOptions
{
    public static void Apply(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.Converters.Add(new IsoMillisecondDateTimeConverter());
    }

    public static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions();
        Apply(options);
        return options;
    }
}
