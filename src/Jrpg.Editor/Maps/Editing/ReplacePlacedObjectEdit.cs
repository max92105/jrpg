using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Replaces one placed object's record, keeping its place in the placement order (decision 513).</summary>
/// <param name="Before">The record before; the same uid as <paramref name="After"/>.</param>
/// <param name="After">The record after.</param>
internal sealed record ReplacePlacedObjectEdit(PlacedObject Before, PlacedObject After) : IMapEdit
{
    public void Apply(MapDraft draft) => draft.ReplaceObject(After);

    public void Revert(MapDraft draft) => draft.ReplaceObject(Before);
}
