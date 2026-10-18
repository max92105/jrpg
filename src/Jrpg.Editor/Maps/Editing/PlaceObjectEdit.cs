using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Places an object on top of the others, last in placement order.</summary>
internal sealed record PlaceObjectEdit(PlacedObject Placed) : IMapEdit
{
    public void Apply(MapDraft draft) => draft.InsertObject(draft.Objects.Count, Placed);

    // Undo runs in reverse order, so the object is still the last one placed.
    public void Revert(MapDraft draft) => draft.RemoveObjectAt(draft.Objects.Count - 1);
}
