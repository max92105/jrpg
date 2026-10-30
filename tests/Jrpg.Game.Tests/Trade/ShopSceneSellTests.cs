using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Core.Trade;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// Selling (decisions 378, 379): a stack through the picker at once, a piece only after a question with its price
// (decision 380); Cancel wins when nested, and a row not for sale shows its reason and does nothing.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneSellTests
{
    private static readonly IReadOnlyDictionary<string, ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void ConfirmInPicker_Stack_SellsTheCountAndAddsGold()
    {
        var (shop, session) = ShopOnTheSellList();
        var item = shop.SelectedRow!.Item;
        Assert.Null(shop.SelectedRow.Piece);
        var owned = session.Inventory.QuantityOf(item.Id);
        var gold = session.Party.Purse.Gold;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        var total = session.Sales.QuoteSale(item, 3).Gold;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(shop.Picker);
        Assert.Equal(gold + total, session.Party.Purse.Gold);
        Assert.Equal(owned - 3, session.Inventory.QuantityOf(item.Id));
        Assert.Equal(_messages.Format(MessageIds.ShopSoldCount, new MessageArgument("name", item.Name), MessageArgument.Number("count", 3)), shop.Notice);
    }

    [Fact]
    public void Confirm_Piece_AsksWithItsPrice()
    {
        var (shop, session) = ShopOnTheRolledSword();
        var sword = shop.SelectedRow!.Piece!;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var prompt = shop.Prompt!;
        Assert.Equal(_messages.Format(MessageIds.ShopSellConfirm, new MessageArgument("name", PieceName.Of(sword, _messages))), prompt.Question);
        Assert.Equal(session.Sales.QuoteSale(sword).Gold, prompt.Price);
        Assert.False(prompt.IsYesSelected);
        Assert.Contains(session.Inventory.Gear, piece => ReferenceEquals(piece, sword));
    }

    [Fact]
    public void PromptYes_SellsThePiece()
    {
        var (shop, session) = ShopOnTheRolledSword();
        var sword = shop.SelectedRow!.Piece!;
        var gold = session.Party.Purse.Gold;
        var price = session.Sales.QuoteSale(sword).Gold;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(shop.Prompt);
        Assert.DoesNotContain(session.Inventory.Gear, piece => ReferenceEquals(piece, sword));
        Assert.Equal(gold + price, session.Party.Purse.Gold);
        Assert.Equal(_messages.Format(MessageIds.ShopSold, new MessageArgument("name", PieceName.Of(sword, _messages))), shop.Notice);
    }

    [Fact]
    public void PromptNo_KeepsThePiece()
    {
        var (shop, session) = ShopOnTheRolledSword();
        var sword = shop.SelectedRow!.Piece!;
        var gold = session.Party.Purse.Gold;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(shop.Prompt);
        Assert.Contains(session.Inventory.Gear, piece => ReferenceEquals(piece, sword));
        Assert.Equal(gold, session.Party.Purse.Gold);
        Assert.Null(shop.Notice);
    }

    [Fact]
    public void Cancel_InPrompt_ClosesThePromptOnly()
    {
        var (shop, _) = ShopOnTheRolledSword();
        var row = shop.SelectedRow;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.Null(shop.Prompt);
        Assert.False(shop.IsOnTabs);
        Assert.Equal(ShopTab.Sell, shop.Tab);
        Assert.Same(row, shop.SelectedRow);
    }

    [Fact]
    public void Confirm_NotForSale_DoesNothing()
    {
        var (shop, session) = ShopOnTheSellList(session => session.Inventory.Add(_items["scrap"], 3));
        MoveDownUntil(shop, row => row.Item.Id == "scrap");
        Assert.Equal(TradeRefusal.NotForSale, shop.SelectedRow!.Refusal);
        var gold = session.Party.Purse.Gold;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(shop.Picker);
        Assert.Null(shop.Prompt);
        Assert.Equal(3, session.Inventory.QuantityOf("scrap"));
        Assert.Equal(gold, session.Party.Purse.Gold);
        Assert.Equal(_messages.Text(MessageIds.ShopRefusedNotForSale), shop.Notice);
    }

    // The rolled sword is the last pickup, so the last row; selling it moves the cursor to the piece before it.
    [Fact]
    public void PromptYes_LastRow_CursorMovesToTheNewLast()
    {
        var (shop, _) = ShopOnTheRolledSword();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        var before = shop.SelectedRow!.Piece;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(before);
        Assert.Same(before, shop.SelectedRow!.Piece);
    }

    // The Sell list with a rolled sword added last, the cursor on it.
    private static (ShopScene Shop, GameSession Session) ShopOnTheRolledSword()
    {
        var sword = GearPiece.Rolled(_items["bronze_sword"], Rarity.Magic, [new RolledAffix("power", StatKind.Power, 2)]);
        var (shop, session) = ShopOnTheSellList(session => session.Inventory.AddGear(sword));
        MoveDownUntil(shop, row => ReferenceEquals(row.Piece, sword));
        return (shop, session);
    }

    // Right on the tabs shows Sell, Confirm enters its list on the first owned stack.
    private static (ShopScene Shop, GameSession Session) ShopOnTheSellList(Action<GameSession>? own = null)
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        own?.Invoke(session);
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        Assert.True(shop.SelectedRow is { Piece: null, IsAvailable: true });
        return (shop, session);
    }

    // Fails rather than loops forever when no row matches: the cursor wraps, so coming back to the start means none.
    private static void MoveDownUntil(ShopScene shop, Func<TradeRow, bool> wanted)
    {
        var start = shop.SelectedRow;
        while (shop.SelectedRow is not { } row || !wanted(row))
        {
            shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));
            Assert.NotSame(start, shop.SelectedRow);
        }
    }
}
