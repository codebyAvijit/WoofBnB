namespace WoofBnB.Domain.ValueObjects;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.model.js's workingHours sub-document:
/// { start, end }, both plain required strings with no format validation in Node
/// (server/src/modules/petsitter/petsitter.validation.js:25-29 only checks z.string()).
/// Kept as a value object rather than TimeOnly so it echoes whatever the client sent,
/// exactly like the Node model does.
/// </summary>
public sealed class WorkingHours
{
    public required string Start { get; set; }

    public required string End { get; set; }
}
