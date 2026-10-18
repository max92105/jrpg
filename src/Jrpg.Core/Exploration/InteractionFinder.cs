using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>Finds the object event the player can start by facing an object. Cell events are found by cell instead.</summary>
internal sealed class InteractionFinder(ActiveMap map, ExplorationRules rules)
{
    /// <summary>How far around an object's bounds a probe still counts as reaching it, in tiles (legacy value).</summary>
    public const double InteractMargin = 0.1;

    /// <summary>
    /// Probes <c>InteractReach</c> tiles in front of the player. Among the objects whose bounds (plus the margin) hold
    /// the probe, the last placed is checked first, as it's drawn on top. The first event of that activation that can
    /// run on the object's current definition, and isn't a "once" event already done, wins.
    /// </summary>
    public TriggeredEvent? FindInFront(Point2D position, Facing facing, EventActivation activation)
    {
        var probe = ProbeInFront(position, facing);
        var placedObjects = map.Definition.Layout.Objects;
        for (var index = placedObjects.Count - 1; index >= 0; index--)
        {
            var placed = placedObjects[index];
            if (placed.Events.Count == 0)
            {
                continue;
            }

            var definition = map.CurrentDefinitionOf(placed);
            if (!IsReached(probe, placed, definition))
            {
                continue;
            }

            for (var eventIndex = 0; eventIndex < placed.Events.Count; eventIndex++)
            {
                var mapEvent = placed.Events[eventIndex];
                if (mapEvent.Activation == activation && mapEvent.CanRunOn(definition.Id) && !(mapEvent.IsOnce && map.IsDone(mapEvent)))
                {
                    return new TriggeredEvent(mapEvent, placed);
                }
            }
        }

        return null;
    }

    private static bool IsReached(Point2D probe, PlacedObject placed, ObjectDefinition definition) =>
        probe.X >= placed.Position.X - InteractMargin && probe.X <= placed.Position.X + definition.Width + InteractMargin
        && probe.Y >= placed.Position.Y - InteractMargin && probe.Y <= placed.Position.Y + definition.Height + InteractMargin;

    private Point2D ProbeInFront(Point2D position, Facing facing)
    {
        var reach = rules.InteractReach;
        return facing switch
        {
            Facing.Up => position with { Y = position.Y - reach },
            Facing.Down => position with { Y = position.Y + reach },
            Facing.Left => position with { X = position.X - reach },
            _ => position with { X = position.X + reach },
        };
    }
}
