using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Inventory page's list: the shown category's rows, their scrolling cursor and the gliding highlight. Rebuilt when
/// the category or the items change, so drawing formats nothing.
/// </summary>
internal sealed class InventoryList
{
    private readonly GameSession _session;
    private readonly ContentCatalog _catalog;
    private readonly SpriteFontBase _bodyFont;
    private InventoryRow[] _rows = [];

    /// <param name="session">Whose inventory is listed.</param>
    /// <param name="catalog">Item names, icons and descriptions.</param>
    /// <param name="bodyFont">Measures the quantities once, when the rows are built.</param>
    public InventoryList(GameSession session, ContentCatalog catalog, SpriteFontBase bodyFont)
    {
        _session = session;
        _catalog = catalog;
        _bodyFont = bodyFont;
    }

    public IReadOnlyList<InventoryRow> Rows => _rows;

    /// <summary>The cursor over the rows; null when the category is empty.</summary>
    public ListCursor? Cursor { get; private set; }

    /// <summary>The highlight gliding to the cursor's row.</summary>
    public SelectionMotion Selection { get; } = new();

    /// <summary>The items on show, in order.</summary>
    public IReadOnlyList<string> ShownItemIds => [.. _rows.Select(row => row.ItemId)];

    /// <summary>The selected row's index; null when the category is empty.</summary>
    public int? SelectedRow => Cursor?.Selected;

    /// <summary>The selected row's item; null when the category is empty.</summary>
    public InventoryRow? SelectedItem => Cursor is null ? null : _rows[Cursor.Selected];

    public void Advance(TimeSpan delta) => Selection.Advance(delta);

    /// <summary>
    /// Lists the category's items. The cursor goes to <paramref name="selectedRow"/>, or the last row once the list got
    /// shorter, and the highlight jumps there.
    /// </summary>
    public void Show(InventoryCategory category, int selectedRow)
    {
        var shown = InventoryFilter.Select(category, _session.Inventory.Entries, _catalog.Items);
        _rows = [.. shown.Select(RowOf)];
        if (_rows.Length == 0)
        {
            Cursor = null;
            return;
        }

        Cursor = new ListCursor(_rows.Length, _ => true, InventoryListView.VisibleRows);
        Cursor.Select(selectedRow);
        Selection.JumpTo(Cursor);
    }

    /// <summary>Lists the category again after the items changed, keeping the cursor on its row.</summary>
    public void Refresh(InventoryCategory category) => Show(category, SelectedRow ?? 0);

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

    private InventoryRow RowOf(InventoryEntry entry)
    {
        var item = _catalog.Items[entry.ItemId];
        var quantity = MeasuredText.Of(_bodyFont, NumberText.Of(entry.Quantity));
        return new InventoryRow(item.Id, item.Name, item.IconFile, quantity, item.Description, _session.FieldItems.CanUse(item));
    }
}
