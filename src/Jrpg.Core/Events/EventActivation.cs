namespace Jrpg.Core.Events;

/// <summary>What starts a map event.</summary>
public enum EventActivation
{
    /// <summary>The player faces the object and presses the confirm button (a chest, a sign).</summary>
    Interact,

    /// <summary>The player walks into the object or onto the cell (a door, a warp tile).</summary>
    Touch,

    /// <summary>The player arrives on the map: a new game, a warp, a load. The cutscene trigger (ADR 0016).</summary>
    Arrive,
}
