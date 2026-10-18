namespace Jrpg.Core.Content;

/// <summary>How the player moves on a map. Tuning values: change them in <c>rules.json</c>, not here.</summary>
public sealed record ExplorationRules
{
    /// <summary>Tiles per second at full tilt.</summary>
    public required double WalkSpeed { get; init; }

    /// <summary>Half the width of the player's square body, in tiles.</summary>
    public required double PlayerRadius { get; init; }

    /// <summary>How far in front of the player an object can be interacted with, in tiles.</summary>
    public required double InteractReach { get; init; }
}
