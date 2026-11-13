using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.World;

namespace Jrpg.Core.Exploration;

/// <summary>
/// The map the player is on, as the player changed it: swapped objects and events already done; the one gate that says
/// whether an event may start now (ADR 0033); and the visit's cutscene walks, where each walked NPC stands now, kept
/// only until the map is left (ADR 0044, decision 574: this object is built anew on every entry).
/// </summary>
internal sealed class ActiveMap(
    MapDefinition definition,
    IReadOnlyDictionary<string, ObjectDefinition> objects,
    WalkabilityGrid walkability,
    WorldState world)
{
    private readonly Dictionary<string, NpcWalk> _walks = new(StringComparer.Ordinal);

    public MapDefinition Definition { get; } = definition;

    /// <summary>
    /// An NPC's pose before the facing rule: its walk's, or standing where it was placed, facing as placed.
    /// </summary>
    public NpcPose PoseOf(PlacedObject placed) =>
        _walks.TryGetValue(placed.Uid, out var walk)
            ? new NpcPose(walk.Position, walk.Facing, walk.IsFinished ? TimeSpan.Zero : walk.Walked, !walk.IsFinished)
            : new NpcPose(placed.Position, placed.Facing ?? Facing.Down, TimeSpan.Zero, IsWalking: false);

    /// <summary>Whether the named NPC's walk is still under way.</summary>
    public bool IsWalking(string uid) => _walks.TryGetValue(uid, out var walk) && !walk.IsFinished;

    /// <summary>Starts a step's walk from where the NPC stands now, facing as it does now (decision 578).</summary>
    public void StartWalk(PlacedObject placed, MoveNpcStep step)
    {
        // A skip runs past a chain of move steps without advancing any: each starts where the one before ends.
        if (_walks.TryGetValue(placed.Uid, out var previous))
        {
            previous.Finish();
        }

        var pose = PoseOf(placed);
        _walks[placed.Uid] = new NpcWalk(pose.Position, pose.Facing, step.Path, Definition.Layout);
    }

    /// <summary>Moves every walk on by the Game's delta at the walk speed; finished ones stay put.</summary>
    public void AdvanceWalks(TimeSpan delta, double cellsPerSecond)
    {
        foreach (var walk in _walks.Values)
        {
            walk.Advance(delta, cellsPerSecond);
        }
    }

    /// <summary>Every walk jumps to its end: a skipped cutscene (decision 576).</summary>
    public void FinishWalks()
    {
        foreach (var walk in _walks.Values)
        {
            walk.Finish();
        }
    }

    public WalkabilityGrid Walkability { get; } = walkability;

    public ObjectDefinition CurrentDefinitionOf(PlacedObject placed) =>
        objects[world.SwappedObjectId(Definition.Id, placed.Uid) ?? placed.ObjectId];

    public void SwapObject(PlacedObject placed, string objectId) =>
        world.SwapObject(new ObjectSwap(Definition.Id, placed.Uid, objectId));

    /// <returns>The touch event on that cell, or null. Called only when the player enters a new cell.</returns>
    public CellEvent? CellEventAt(GridCell cell)
    {
        var cellEvents = Definition.Layout.CellEvents;
        for (var index = 0; index < cellEvents.Count; index++)
        {
            if (cellEvents[index].Cell == cell && cellEvents[index].Event.Activation == EventActivation.Touch)
            {
                return cellEvents[index];
            }
        }

        return null;
    }

    public bool IsDone(MapEvent mapEvent) => world.IsEventDone(KeyOf(mapEvent));

    /// <summary>
    /// Whether the event may start now, whatever its trigger (ADR 0033): for this leader (decision 185), not a "once"
    /// event already done, and its quest condition met by the world's log. The object's look is
    /// <see cref="MapEvent.CanRunOn"/>.
    /// </summary>
    public bool CanStart(MapEvent mapEvent, string leaderId) =>
        mapEvent.IsForLeader(leaderId)
        && !(mapEvent.IsOnce && IsDone(mapEvent))
        && (mapEvent.OnlyWhenQuest is not { } gate || gate.IsMetBy(world.Quests));

    public void MarkDone(MapEvent mapEvent) => world.MarkEventDone(KeyOf(mapEvent));

    private MapEventKey KeyOf(MapEvent mapEvent) => new(Definition.Id, mapEvent.Id);
}
