namespace Jrpg.ContentConverter.Events;

/// <summary>A legacy tile trigger, run when the player steps on the tile. Only the fields of its <see cref="Type"/> are set.</summary>
internal sealed record LegacyTriggerJson
{
    public required LegacyTriggerType Type { get; init; }

    /// <summary>A legacy map file name, such as <c>Forest.json</c>.</summary>
    public string? TargetMap { get; init; }

    public int TargetX { get; init; }

    public int TargetY { get; init; }

    public string? SpeakerName { get; init; }

    public string? PortraitPath { get; init; }

    public IReadOnlyList<string>? Lines { get; init; }

    public string? ItemId { get; init; }
}
