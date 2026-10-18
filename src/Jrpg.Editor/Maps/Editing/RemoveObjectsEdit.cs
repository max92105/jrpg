using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Removes placed objects as one undo step. Undo puts each back at its index, so the placement order, and with it the
/// draw order, is as before.
/// </summary>
internal sealed class RemoveObjectsEdit : IMapEdit
{
    // Lowest index first.
    private readonly List<(int Index, PlacedObject Placed)> _removed;

    /// <param name="objects">The map's objects in placement order.</param>
    /// <param name="uids">The uids of those to remove.</param>
    public RemoveObjectsEdit(IReadOnlyList<PlacedObject> objects, IReadOnlyCollection<string> uids)
    {
        _removed = [.. objects.Select((placed, index) => (index, placed)).Where(entry => uids.Contains(entry.placed.Uid))];
    }

    public int Count => _removed.Count;

    // Highest index first, so removing one doesn't shift those still to remove.
    public void Apply(MapDraft draft)
    {
        for (var entry = _removed.Count - 1; entry >= 0; entry--)
        {
            draft.RemoveObjectAt(_removed[entry].Index);
        }
    }

    public void Revert(MapDraft draft)
    {
        foreach (var (index, placed) in _removed)
        {
            draft.InsertObject(index, placed);
        }
    }
}
