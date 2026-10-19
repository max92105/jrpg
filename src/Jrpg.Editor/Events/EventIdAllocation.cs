using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Events;

/// <summary>New event ids for a placed object: its uid and object id, and the event ids it has used, which new events skip.</summary>
internal sealed record EventIdAllocation(string Uid, string ObjectId, IReadOnlyCollection<string> UsedEventIds)
{
    /// <summary>Ids for new events, in order, each the next one the object hasn't used.</summary>
    public IReadOnlyList<string> NewEventIds(int count)
    {
        var used = UsedEventIds.ToList();
        for (var made = 0; made < count; made++)
        {
            used.Add(EventIds.Next(Uid, used));
        }

        return used[^count..];
    }
}
