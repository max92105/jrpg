using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Copies of placed objects for a map (decision 512): each gets a uid no object there has, and each event an id no
/// event there has, cell events included, so while the originals are on the map a save's memory of them (once events,
/// swaps) doesn't carry over; a cut original pasted back may take its old uid, as a move would. Everything else
/// (object, layer, gates, steps) is kept, and the group keeps its layout.
/// </summary>
internal static class PlacedObjectCopies
{
    /// <param name="copied">The objects copied, in their placement order.</param>
    /// <param name="target">The map they are pasted on, as it is now.</param>
    /// <param name="topLeft">Where the group's top-left corner (<see cref="TopLeftOf"/>) lands, in map cells.</param>
    /// <returns>The copies, in the copied order.</returns>
    public static IReadOnlyList<PlacedObject> Make(IReadOnlyList<PlacedObject> copied, MapLayout target, Point2D topLeft)
    {
        var corner = TopLeftOf(copied);
        var takenUids = target.Objects.Select(placed => placed.Uid).ToHashSet();
        var usedEventIds = target.AllEvents.Select(mapEvent => mapEvent.Id).ToHashSet();
        var copies = new List<PlacedObject>(copied.Count);
        foreach (var original in copied)
        {
            var uid = PlacedObjectUids.Next(original.ObjectId, takenUids);
            takenUids.Add(uid);
            copies.Add(original with
            {
                Uid = uid,
                Position = new Point2D(topLeft.X + (original.Position.X - corner.X), topLeft.Y + (original.Position.Y - corner.Y)),
                Events = [.. original.Events.Select(mapEvent => mapEvent with { Id = EventIdFor(mapEvent.Id, original.Uid, uid, usedEventIds) })],
            });
        }

        return copies;
    }

    /// <summary>The group's top-left corner: the smallest x and the smallest y of its objects' positions.</summary>
    public static Point2D TopLeftOf(IReadOnlyList<PlacedObject> objects) =>
        new(objects.Min(placed => placed.Position.X), objects.Min(placed => placed.Position.Y));

    // {oldUid}_{n} keeps its n under the new uid while that id is free; another id, or a taken one, gets the next free.
    private static string EventIdFor(string oldId, string oldUid, string newUid, HashSet<string> used)
    {
        var kept = NumberedIds.HighestNumber($"{oldUid}_", [oldId]) is { } number ? $"{newUid}_{number}" : null;
        var id = kept is not null && !used.Contains(kept) ? kept : EventIds.Next(newUid, used);
        used.Add(id);
        return id;
    }
}
