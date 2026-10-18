using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Moves one placed object; its place in the placement order stays.</summary>
/// <param name="Uid">The object moved.</param>
/// <param name="From">Its top-left corner before, in map cells.</param>
/// <param name="To">Its top-left corner after, in map cells.</param>
internal sealed record MoveObjectEdit(string Uid, Point2D From, Point2D To) : IMapEdit
{
    public void Apply(MapDraft draft) => draft.MoveObject(Uid, To);

    public void Revert(MapDraft draft) => draft.MoveObject(Uid, From);
}
