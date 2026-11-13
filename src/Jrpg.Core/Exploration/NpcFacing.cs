using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>
/// What an NPC faces (ADR 0044, decision 577): the player while an event of its own runs, whatever its trigger and the
/// shop or Forge it opened; otherwise its placed facing, down when it has none (decision 572). Worked out at draw time
/// from what already exists, nothing saved.
/// </summary>
internal static class NpcFacing
{
    /// <param name="placed">The NPC drawn.</param>
    /// <param name="speaking">The object whose event runs now; null while none runs or a cell's does.</param>
    /// <param name="playerFacing">Which way the player looks; the NPC looks back.</param>
    public static Facing Of(PlacedObject placed, PlacedObject? speaking, Facing playerFacing) =>
        // The same placed object, not the same uid: uids repeat across maps, and an event warping mid-way keeps its
        // source from the map it started on. A swap keeps the placed object (the world remembers it by uid).
        ReferenceEquals(speaking, placed) ? Facings.Opposite(playerFacing) : placed.Facing ?? Facing.Down;
}
