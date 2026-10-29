using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>
/// The Forge's list of the owned pieces (decision 369: worn ones aren't owned), in pickup order, with their scrolling
/// cursor and the gliding highlight. Built when the items change, so drawing formats nothing. A row index is a gear
/// index: the pieces are <see cref="Inventory.Gear"/> as it stands.
/// </summary>
internal sealed class ForgeList(Inventory inventory)
{
    private GearPiece[] _pieces = [];

    public IReadOnlyList<GearPiece> Pieces => _pieces;

    /// <summary>The cursor over the pieces; null when none is owned.</summary>
    public ListCursor? Cursor { get; private set; }

    /// <summary>The highlight gliding to the cursor's row.</summary>
    public SelectionMotion Selection { get; } = new();

    /// <summary>The selected row's piece; null when none is owned.</summary>
    public GearPiece? SelectedPiece => Cursor is null ? null : _pieces[Cursor.Selected];

    public void Advance(TimeSpan delta) => Selection.Advance(delta);

    /// <summary>
    /// Lists the owned pieces. The cursor goes to <paramref name="selectedRow"/>, or the last row once the list got
    /// shorter, and the highlight jumps there.
    /// </summary>
    public void Show(int selectedRow)
    {
        _pieces = [.. inventory.Gear];
        if (_pieces.Length == 0)
        {
            Cursor = null;
            return;
        }

        Cursor = new ListCursor(_pieces.Length, _ => true, ForgeView.VisibleRows);
        Cursor.Select(selectedRow);
        Selection.JumpTo(Cursor);
    }

    public void MoveUp() => Move(cursor => cursor.MoveUp());

    public void MoveDown() => Move(cursor => cursor.MoveDown());

    private void Move(Action<ListCursor> step)
    {
        if (Cursor is not { } cursor)
        {
            return;
        }

        step(cursor);
        Selection.Follow(cursor);
    }
}
