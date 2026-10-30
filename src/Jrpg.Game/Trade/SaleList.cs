using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>
/// The Sell list: the owned stacks in pickup order, then the owned pieces in pickup order (worn ones aren't owned,
/// decision 369), with its scrolling cursor and gliding highlight. Rebuilt when the inventory changes, so drawing
/// formats nothing. Each row is quoted by Core for one copy, so one not for sale is shown greyed (decision 378).
/// </summary>
internal sealed class SaleList
{
    private readonly ItemSales _sales;
    private readonly Inventory _inventory;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private readonly SpriteFontBase _font;
    private TradeRow[] _rows = [];

    /// <param name="sales">The session's sales, which quote each row.</param>
    /// <param name="inventory">The party's: what can be sold.</param>
    /// <param name="items">The catalog's items, for a stack's definition.</param>
    /// <param name="font">Measures the names and prices once, when the rows are built.</param>
    public SaleList(ItemSales sales, Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items, SpriteFontBase font)
    {
        _sales = sales;
        _inventory = inventory;
        _items = items;
        _font = font;
        Refresh();
    }

    public IReadOnlyList<TradeRow> Rows => _rows;

    /// <summary>The cursor over the rows and the highlight gliding to it; no cursor while nothing is owned.</summary>
    public RowSelection Selection { get; } = new();

    public TradeRow? SelectedRow => Selection.SelectedIndex is { } index ? _rows[index] : null;

    /// <summary>
    /// Builds the rows again; the cursor stays on its row, or the last one once the list got shorter.
    /// </summary>
    public void Refresh()
    {
        _rows = [.. _inventory.Entries.Select(StackRow), .. _inventory.Gear.Select(PieceRow)];
        Selection.Reset(_rows.Length, Selection.SelectedIndex ?? 0, ShopListView.VisibleRows);
    }

    private TradeRow StackRow(InventoryEntry entry)
    {
        var item = _items[entry.ItemId];
        var quote = _sales.QuoteSale(item, 1);
        return new TradeRow(item, null, Measure(item.Name), Measure(NumberText.Of(entry.Quantity)), Measure(NumberText.Of(quote.Gold)), quote.Refusal);
    }

    // A piece is one of a kind: no count beside it.
    private TradeRow PieceRow(GearPiece piece)
    {
        var quote = _sales.QuoteSale(piece);
        return new TradeRow(piece.Base, piece, Measure(piece.Base.Name), Measure(string.Empty), Measure(NumberText.Of(quote.Gold)), quote.Refusal);
    }

    private MeasuredText Measure(string text) => MeasuredText.Of(_font, text);
}
