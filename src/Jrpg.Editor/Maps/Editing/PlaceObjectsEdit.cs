using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Places several objects on top of the others, in order, as one step (a paste, decision 512).</summary>
internal sealed record PlaceObjectsEdit(IReadOnlyList<PlacedObject> Placed) : IMapEdit
{
    public void Apply(MapDraft draft)
    {
        foreach (var placed in Placed)
        {
            draft.InsertObject(draft.Objects.Count, placed);
        }
    }

    // Undo runs in reverse order, so they are still the last ones placed.
    public void Revert(MapDraft draft)
    {
        for (var removed = 0; removed < Placed.Count; removed++)
        {
            draft.RemoveObjectAt(draft.Objects.Count - 1);
        }
    }
}
