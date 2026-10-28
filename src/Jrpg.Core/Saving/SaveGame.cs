using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.World;

namespace Jrpg.Core.Saving;

/// <summary>
/// Everything a save restores, as plain ids and values (ADR 0007). Built by <c>GameSession.Capture</c> and turned back
/// into a session by <c>GameSession.Restore</c>; the random state isn't kept (developer decision 43).
/// </summary>
public sealed record SaveGame
{
    /// <summary>When the player saved, from the Game's clock; Core never reads the time itself.</summary>
    public required DateTimeOffset SavedAt { get; init; }

    public required TimeSpan PlayTime { get; init; }

    public required MapLocation Location { get; init; }

    /// <summary>Every recruited member, active or not.</summary>
    public required IReadOnlyList<PartyMemberProgress> Members { get; init; }

    /// <summary>The active party in order; the first is the leader.</summary>
    public required IReadOnlyList<string> ActiveMemberIds { get; init; }

    /// <summary>Owned stacks; gear is in <see cref="Gear"/>, worn gear in <see cref="Members"/> (ADR 0003).</summary>
    public required IReadOnlyList<InventoryEntry> Inventory { get; init; }

    /// <summary>Owned gear, one piece each, in pickup order (ADR 0029).</summary>
    public required IReadOnlyList<SavedGear> Gear { get; init; }

    public required IReadOnlyList<MapEventKey> CompletedEvents { get; init; }

    public required IReadOnlyList<ObjectSwap> ObjectSwaps { get; init; }
}
