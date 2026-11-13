using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Data.Maps;

/// <summary>
/// A <c>moveNpc</c> step's rules (ADR 0044): a path of 1 to <see cref="MoveNpcStep.MaxSteps"/> steps, naming a placed
/// NPC of the step's own map, every cell of the path on the map. Walls and objects on the way are allowed (decision
/// 575). The caller names the event and the step's position.
/// </summary>
internal static class MoveNpcStepValidator
{
    /// <param name="move">The step.</param>
    /// <param name="layout">
    /// The map whose event holds the step; the objects are already checked against the catalog.
    /// </param>
    /// <param name="objects">Every object definition, to tell an NPC from a tile object.</param>
    /// <returns>What is wrong with the step, or null if nothing is.</returns>
    public static string? FindProblem(MoveNpcStep move, MapLayout layout, IReadOnlyDictionary<string, ObjectDefinition> objects)
    {
        if (move.Path.Count < 1 || move.Path.Count > MoveNpcStep.MaxSteps)
        {
            return $"moveNpc path must hold 1 to {MoveNpcStep.MaxSteps} steps (was {move.Path.Count}).";
        }

        var placed = layout.Objects.FirstOrDefault(candidate => candidate.Uid == move.ObjectUid);
        if (placed is null)
        {
            return $"moveNpc names '{move.ObjectUid}', which isn't placed on this map.";
        }

        if (objects[placed.ObjectId].FieldSpriteFile is null)
        {
            return $"moveNpc names '{move.ObjectUid}' ({placed.ObjectId}), but only an NPC walks.";
        }

        return FindPathProblem(move.Path, GridCell.Containing(placed.Position), layout);
    }

    private static string? FindPathProblem(IReadOnlyList<Facing> path, GridCell start, MapLayout layout)
    {
        var cell = start;
        for (var index = 0; index < path.Count; index++)
        {
            cell = Facings.Step(cell, path[index]);
            if (!layout.Contains(cell))
            {
                return $"moveNpc path step {index} leaves the map at ({cell.X}, {cell.Y}).";
            }
        }

        return null;
    }
}
