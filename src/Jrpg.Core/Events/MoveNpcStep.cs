using Jrpg.Core.Geometry;

namespace Jrpg.Core.Events;

/// <summary>
/// Walks a placed NPC of the event's own map along a path of one-cell steps while the event waits (ADR 0044, decision
/// 573); a presentation step, so a skip runs past it with the NPC at the path's end (decision 576). The path is
/// relative to where the NPC stands, so a second run starts from the first's end (decision 578).
/// </summary>
/// <param name="ObjectUid">The placed NPC, by uid on the event's map.</param>
/// <param name="Path">Each step's direction, in order, 1 to <see cref="MaxSteps"/>.</param>
public sealed record MoveNpcStep(string ObjectUid, IReadOnlyList<Facing> Path) : EventStep
{
    /// <summary>The longest path a step may hold; a technical bound.</summary>
    public const int MaxSteps = 64;

    public override bool IsPresentation => true;

    // A record compares lists by reference; two steps with the same path are the same step.
    public bool Equals(MoveNpcStep? other) =>
        other is not null && ObjectUid == other.ObjectUid && Path.SequenceEqual(other.Path);

    public override int GetHashCode() => HashCode.Combine(ObjectUid, Path.Count);
}
