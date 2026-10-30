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
    private TradeRow[] _rows = [];

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

    public IReadOnlyList<TradeRow> Rows => _rows;

    /// <summary>The cursor over the rows and the highlight gliding to it; no cursor when the stock is empty.</summary>
    public RowSelection Selection { get; } = new();

    public TradeRow? SelectedRow => Selection.SelectedIndex is { } index ? _rows[index] : null;

    /// <summary>
    /// Builds the rows again; the cursor stays on its row, or the last one once the list got shorter.
    /// </summary>
    public void Refresh()
    {
        _rows = [.. _shop.Stock.Definition.ItemIds.SelectMany(RowsOf)];
        Selection.Reset(_rows.Length, Selection.SelectedIndex ?? 0, ShopListView.VisibleRows);
    }

    // A stack entry is one row; a gear entry is one row per piece still in the stock (none once all are bought).
    private IEnumerable<TradeRow> RowsOf(string itemId)
    {
        var stack = _shop.Stock.Stacks.FirstOrDefault(item => item.Id == itemId);
        if (stack is not null)
        {
            return [StackRow(stack)];
        }

        return _shop.Stock.Pieces.Where(piece => piece.Base.Id == itemId).Select(PieceRow);
    }

    private TradeRow StackRow(ItemDefinition item)
    {
        var quote = _shop.QuoteBuy(item, 1);
        return new TradeRow(item, null, Measure(item.Name), Measure(NumberText.Of(_inventory.QuantityOf(item.Id))), Measure(NumberText.Of(quote.Gold)), quote.Refusal);
    }

    private TradeRow PieceRow(GearPiece piece)
    {
        var quote = _shop.QuoteBuy(piece);
        return new TradeRow(piece.Base, piece, Measure(piece.Base.Name), Measure(NumberText.Of(_inventory.GearCountOf(piece.Base.Id))), Measure(NumberText.Of(quote.Gold)), quote.Refusal);
    }

    private MeasuredText Measure(string text) => MeasuredText.Of(_font, text);
}
