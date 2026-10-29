using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>
/// The Forge's list of the owned pieces (decision 369: worn ones aren't owned), in pickup order, with their scrolling
/// cursor and the gliding highlight. Built when the items change, so drawing formats nothing. A row index is a gear
/// index: the rows are <see cref="Inventory.Gear"/> as it stands.
/// </summary>
/// <param name="inventory">Whose pieces are listed.</param>
/// <param name="messages">A piece's rarity and affix names.</param>
/// <param name="bodyFont">Measures the names once, when the rows are built.</param>
internal sealed class ForgeList(Inventory inventory, UiMessages messages, SpriteFontBase bodyFont)
{
    private ForgeRow[] _rows = [];

    public IReadOnlyList<ForgeRow> Rows => _rows;

    /// <summary>The cursor over the rows; null when no piece is owned.</summary>
    public ListCursor? Cursor { get; private set; }

    /// <summary>The highlight gliding to the cursor's row.</summary>
    public SelectionMotion Selection { get; } = new();

    /// <summary>The selected row; null when no piece is owned.</summary>
    public ForgeRow? SelectedRow => Cursor is null ? null : _rows[Cursor.Selected];

    /// <summary>The selected row's piece; null when no piece is owned.</summary>
    public GearPiece? SelectedPiece => SelectedRow?.Piece;

    public void Advance(TimeSpan delta) => Selection.Advance(delta);

    /// <summary>
    /// Lists the owned pieces. The cursor goes to <paramref name="selectedRow"/>, or the last row once the list got
    /// shorter, and the highlight jumps there.
    /// </summary>
    public void Show(int selectedRow)
    {
        _rows = [.. inventory.Gear.Select(RowOf)];
        if (_rows.Length == 0)
        {
            Cursor = null;
            return;
        }

        Cursor = new ListCursor(_rows.Length, _ => true, ForgeListView.VisibleRows);
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

    private ForgeRow RowOf(GearPiece piece) => new(piece, MeasuredText.Of(bodyFont, piece.Base.Name), messages.Text(RarityNames.IdOf(piece.Rarity)), [.. piece.Affixes.Select(affix => AffixText.Of(affix, messages))]);
}
