using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Trade;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>
/// The counter where the party buys and sells (decision 379): the Buy list of the stock, the Sell list of the bag, and
/// the deal made on a highlighted row. A piece is bought at once, leaving the stock for the bag (decision 385); a
/// stack, bought or sold, goes through the <see cref="QuantityPicker"/>; a piece is sold only after a question with its
/// price under it (decisions 379, 380), as destroying one asks at the Forge. While the picker or the question is open
/// the counter takes every press. Both lists are built again after a deal, since the quotes follow the gold and the
/// bag; the notice names what was dealt until the Shop clears it. It asks its sounds in the list the scene passes each
/// call (decision 435): the coins once Core dealt, however it was confirmed; the picker, a value chosen, ticks on a
/// change and sounds its Cancel; the sale question is silent, its No included.
/// </summary>
internal sealed class ShopCounter
{
    private readonly Shop _shop;
    private readonly ItemSales _sales;
    private readonly UiMessages _messages;
    private OpenDeal? _open;

    /// <param name="shop">The shop opened, from <c>GameSession.ShopFor</c>.</param>
    /// <param name="session">Whose bag is sold from and bought into, and whose sales quote the Sell list.</param>
    /// <param name="text">The rows' fonts and the notices' lines.</param>
    public ShopCounter(Shop shop, GameSession session, ScreenText text)
    {
        _shop = shop;
        _sales = session.Sales;
        _messages = text.Messages;
        Stock = new StockList(shop, session.Inventory, text);
        Sales = new SaleList(session.Sales, session.Inventory, session.Catalog.Items, text);
    }

    public StockList Stock { get; }

    public SaleList Sales { get; }

    /// <summary>The open picker for a stack's count; null while none is open.</summary>
    public QuantityPicker? Picker => _open?.Picker;

    /// <summary>The question before a piece is sold; null while nothing is asked.</summary>
    public ConfirmPrompt? Prompt => _open?.Prompt;

    /// <summary>Whether a picker or a question takes the presses.</summary>
    public bool IsOpen => _open is not null;

    /// <summary>What the last deal bought or sold; null once cleared.</summary>
    public string? Notice { get; private set; }

    public void ClearNotice() => Notice = null;

    /// <summary>Confirm on <paramref name="row"/>, an available row of the Buy list.</summary>
    public void Buy(TradeRow row, AskedSounds sounds)
    {
        if (row.Piece is { } piece)
        {
            if (Settle(_shop.TryBuy(piece), Named(MessageIds.ShopBought, piece)))
            {
                sounds.Ask(UiSoundFiles.Effects.Deal);
            }

            return;
        }

        sounds.Ask(UiSoundFiles.Effects.MenuConfirm);
        OpenPicker(_shop.MostBuyable(row.Item), count => _shop.QuoteBuy(row.Item, count), count => Settle(_shop.TryBuy(row.Item, count), Counted(MessageIds.ShopBoughtCount, row, count)));
    }

    /// <summary>Confirm on <paramref name="row"/>, an available row of the Sell list.</summary>
    public void Sell(TradeRow row, AskedSounds sounds)
    {
        if (row.Piece is { } piece)
        {
            var question = _messages.Format(MessageIds.ShopSellConfirm, new MessageArgument("name", PieceName.Of(piece, _messages)));
            var prompt = new ConfirmPrompt(question) { Price = _sales.QuoteSale(piece).Gold };
            _open = new OpenDeal(null, prompt, () => Settle(_sales.TrySell(piece), Named(MessageIds.ShopSold, piece)));
            return;
        }

        sounds.Ask(UiSoundFiles.Effects.MenuConfirm);
        OpenPicker(_sales.MostSellable(row.Item), count => _sales.QuoteSale(row.Item, count), count => Settle(_sales.TrySell(row.Item, count), Counted(MessageIds.ShopSoldCount, row, count)));
    }

    /// <summary>The open picker's or question's input: Yes settles the deal, No or Cancel drops it.</summary>
    public void Update(InputFrame input, AskedSounds sounds)
    {
        var deal = _open ?? throw new InvalidOperationException("Nothing is open.");
        var count = deal.Picker?.Count ?? 0;
        if (deal.Answer(input) is not { } given)
        {
            sounds.AskMoveIfChanged(count, deal.Picker?.Count ?? 0);
            return;
        }

        _open = null;
        Close(deal, given, sounds);
    }

    // The coins only once Core dealt; the picker's No is its Cancel, the question's No is silent (decision 435).
    private static void Close(OpenDeal deal, ConfirmAnswer given, AskedSounds sounds)
    {
        if (given == ConfirmAnswer.Yes)
        {
            if (deal.Settle())
            {
                sounds.Ask(UiSoundFiles.Effects.Deal);
            }

            return;
        }

        if (deal.Picker is not null)
        {
            sounds.Ask(UiSoundFiles.Effects.MenuCancel);
        }
    }

    // The count is read when the player confirms, not when the picker opens.
    private void OpenPicker(int most, Func<int, TradeQuote> quote, Func<int, bool> settle)
    {
        var picker = new QuantityPicker(most, quote);
        _open = new OpenDeal(picker, null, () => settle(picker.Count));
    }

    // Core refusing here means the quote changed under the open deal; the row's new quote shows instead of a notice.
    // The cursor stays on its row, or the last once a dealt piece's row is gone.
    private bool Settle(bool dealt, string notice)
    {
        if (!dealt)
        {
            return false;
        }

        Stock.Refresh();
        Sales.Refresh();
        Notice = notice;
        return true;
    }

    // A piece is named as the question names it, with its rarity.
    private string Named(string noticeId, GearPiece piece) => _messages.Format(noticeId, new MessageArgument("name", PieceName.Of(piece, _messages)));

    private string Counted(string noticeId, TradeRow row, int count) =>
        _messages.Format(noticeId, new MessageArgument("name", row.Name.Text), MessageArgument.Number("count", count));

    // The picker or the question that takes the presses, and what Yes does; one of the two is open.
    private sealed record OpenDeal(QuantityPicker? Picker, ConfirmPrompt? Prompt, Func<bool> Settle)
    {
        public ConfirmAnswer? Answer(InputFrame input) => Prompt is { } prompt ? prompt.Update(input) : Picker!.Update(input);
    }
}
