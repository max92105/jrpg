using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>
/// Buying from the Buy list (decision 379): Confirm on an available piece takes it at once, the piece leaving the stock
/// for the bag (decision 385); on a stack it opens the <see cref="QuantityPicker"/>, whose Confirm buys the count. Both
/// lists are built again after a purchase, since the quotes follow the gold and can grey other rows; the notice names
/// what was bought (decision 380) until the Shop clears it.
/// </summary>
internal sealed class Purchase
{
    private readonly Shop _shop;
    private readonly StockList _stock;
    private readonly SaleList _sales;
    private readonly UiMessages _messages;
    private (TradeRow Row, QuantityPicker Picker)? _open;

    public Purchase(Shop shop, StockList stock, SaleList sales, UiMessages messages)
    {
        _shop = shop;
        _stock = stock;
        _sales = sales;
        _messages = messages;
    }

    /// <summary>The open picker for a stack's count; null while none is open.</summary>
    public QuantityPicker? Picker => _open?.Picker;

    /// <summary>What the last purchase bought; null once cleared.</summary>
    public string? Notice { get; private set; }

    public void ClearNotice() => Notice = null;

    /// <summary>Confirm on <paramref name="row"/>, an available row of the Buy list.</summary>
    public void Confirm(TradeRow row)
    {
        if (row.Piece is { } piece)
        {
            BuyPiece(row, piece);
            return;
        }

        _open = (row, new QuantityPicker(_shop.MostBuyable(row.Item), count => _shop.QuoteBuy(row.Item, count)));
    }

    /// <summary>
    /// The open picker's input: Cancel closes it, Confirm buys the count, Up and Down change it by one, Left and Right
    /// by ten (decision 377).
    /// </summary>
    public void UpdatePicker(InputFrame input)
    {
        var (row, picker) = _open ?? throw new InvalidOperationException("No picker is open.");
        if (input.IsPressed(GameButton.Cancel))
        {
            _open = null;
            return;
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            BuyStack(row, picker.Count);
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            picker.Up();
        }

        if (input.IsPressed(GameButton.Down))
        {
            picker.Down();
        }

        if (input.IsPressed(GameButton.Right))
        {
            picker.StepUp();
        }

        if (input.IsPressed(GameButton.Left))
        {
            picker.StepDown();
        }
    }

    private void BuyPiece(TradeRow row, GearPiece piece)
    {
        if (!_shop.TryBuy(piece))
        {
            return;
        }

        RefreshLists();
        Notice = _messages.Format(MessageIds.ShopBought, new MessageArgument("name", row.Name.Text));
    }

    // The picker closes either way: Core refusing here means the quote changed under it, so the row's new quote shows.
    private void BuyStack(TradeRow row, int count)
    {
        _open = null;
        if (!_shop.TryBuy(row.Item, count))
        {
            return;
        }

        RefreshLists();
        Notice = _messages.Format(MessageIds.ShopBoughtCount, new MessageArgument("name", row.Name.Text), MessageArgument.Number("count", count));
    }

    // The cursor stays on its row, or the last once a bought piece's row is gone.
    private void RefreshLists()
    {
        _stock.Refresh();
        _sales.Refresh();
    }
}
