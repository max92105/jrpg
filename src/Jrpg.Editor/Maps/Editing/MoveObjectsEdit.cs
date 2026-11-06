namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Moves several placed objects as one step (decision 510): each keeps its exact from and to, so an undo puts back the
/// position it had, not one an offset taken off a free position would round to.
/// </summary>
/// <param name="Moves">One move per object; applied in order, reverted in reverse.</param>
internal sealed record MoveObjectsEdit(IReadOnlyList<MoveObjectEdit> Moves) : IMapEdit
{
    public void Apply(MapDraft draft)
    {
        foreach (var move in Moves)
        {
            move.Apply(draft);
        }
    }

    public void Revert(MapDraft draft)
    {
        foreach (var move in Moves.Reverse())
        {
            move.Revert(draft);
        }
    }
}
