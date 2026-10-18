namespace Jrpg.ContentConverter.Events;

/// <summary>One flat legacy event action: only the fields of its <see cref="Type"/> mean anything.</summary>
internal sealed record LegacyEventActionJson
{
    public required LegacyEventActionType Type { get; init; }

    public string? Speaker { get; init; }

    public string? Portrait { get; init; }

    public IReadOnlyList<string>? Lines { get; init; }

    public string? ItemId { get; init; }

    public int Quantity { get; init; } = 1;

    /// <summary>A legacy int object id; 0 when the action isn't a swap.</summary>
    public int ObjectId { get; init; }

    /// <summary>A legacy map file name, such as <c>Interior1.json</c>.</summary>
    public string? TargetMap { get; init; }

    public int TargetX { get; init; }

    public int TargetY { get; init; }
}
