using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// An object made from map cells put in their place, as one step (decision 515): the cells it took erased, layer by
/// layer, then the object placed on top of the others.
/// </summary>
/// <param name="Erased">Each layer's erased cells, with the tiles they had.</param>
/// <param name="Placed">The object placed where the cells were.</param>
internal sealed record ReplaceCellsWithObjectEdit(IReadOnlyList<SetTilesEdit> Erased, PlacedObject Placed) : IMapEdit
{
    public void Apply(MapDraft draft)
    {
        foreach (var erase in Erased)
        {
            erase.Apply(draft);
        }

        draft.InsertObject(draft.Objects.Count, Placed);
    }

    // In reverse: the object is still the last one placed, then the cells come back.
    public void Revert(MapDraft draft)
    {
        draft.RemoveObjectAt(draft.Objects.Count - 1);
        foreach (var erase in Erased.Reverse())
        {
            erase.Revert(draft);
        }
    }
}
