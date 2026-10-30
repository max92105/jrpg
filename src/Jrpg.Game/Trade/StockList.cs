using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>
/// The Buy list: the shop's stock in its list's order (a stack, or each gear entry's rolled piece in its place), with
/// its scrolling cursor and gliding highlight. Rebuilt when the inventory, the gold or the stock changes, so drawing
/// formats nothing. Each row is quoted by Core for one copy, so a refused one is shown greyed.
/// </summary>
internal sealed class StockList
{
    private readonly Shop _shop;
    private readonly Inventory _inventory;
    private readonly SpriteFontBase _font;
    private StockRow[] _rows = [];

    /// <param name="shop">Whose stock is listed and quotes each row.</param>
    /// <param name="inventory">The party's, for what is owned.</param>
    /// <param name="font">Measures the names and prices once, when the rows are built.</param>
    public StockList(Shop shop, Inventory inventory, SpriteFontBase font)
    {
        _shop = shop;
        _inventory = inventory;
        _font = font;
        Refresh();
    }

    public IReadOnlyList<StockRow> Rows => _rows;

    /// <summary>The cursor over the rows; null when the stock is empty.</summary>
    public ListCursor? Cursor { get; private set; }

    /// <summary>The highlight gliding to the cursor's row.</summary>
    public SelectionMotion Selection { get; } = new();

    public StockRow? SelectedRow => Cursor is null ? null : _rows[Cursor.Selected];

    public void Advance(TimeSpan delta) => Selection.Advance(delta);

    /// <summary>
    /// Builds the rows again; the cursor stays on its row, or the last one once the list got shorter.
    /// </summary>
    public void Refresh()
    {
        var selected = Cursor?.Selected ?? 0;
        _rows = [.. _shop.Stock.Definition.ItemIds.SelectMany(RowsOf)];
        if (_rows.Length == 0)
        {
            Cursor = null;
            return;
        }

        Cursor = new ListCursor(_rows.Length, _ => true, ShopListView.VisibleRows);
        Cursor.Select(selected);
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

    // A stack entry is one row; a gear entry is one row per piece still in the stock (none once all are bought).
    private IEnumerable<StockRow> RowsOf(string itemId)
    {
        var stack = _shop.Stock.Stacks.FirstOrDefault(item => item.Id == itemId);
        if (stack is not null)
        {
            return [StackRow(stack)];
        }

        return _shop.Stock.Pieces.Where(piece => piece.Base.Id == itemId).Select(PieceRow);
    }

    private StockRow StackRow(ItemDefinition item)
    {
        var quote = _shop.QuoteBuy(item, 1);
        return new StockRow(item, null, Measure(item.Name), Measure(NumberText.Of(_inventory.QuantityOf(item.Id))), Measure(NumberText.Of(quote.Gold)), quote.Refusal);
    }

    private StockRow PieceRow(GearPiece piece)
    {
        var quote = _shop.QuoteBuy(piece);
        return new StockRow(piece.Base, piece, Measure(piece.Base.Name), Measure(NumberText.Of(_inventory.GearCountOf(piece.Base.Id))), Measure(NumberText.Of(quote.Gold)), quote.Refusal);
    }

    private MeasuredText Measure(string text) => MeasuredText.Of(_font, text);
}
