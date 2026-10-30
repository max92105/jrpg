using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Core.Trade;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// Confirm on an available piece buys it at once (decision 379): the gold drops, the piece moves from the stock to the
// bag, and a notice names it until the cursor moves. A refused row shows its reason instead, and Confirm does nothing.
// The bag may hold a plain copy equal to a stock piece by value, so the checks are by reference.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneBuyTests
{
    [Fact]
    public void Confirm_AffordablePiece_BuysItAndTakesItsPrice()
    {
        var (shop, session, store) = ShopOnAGearRow();
        var piece = shop.SelectedRow!.Piece!;
        var name = shop.SelectedRow.Name.Text;
        var price = store.QuoteBuy(piece).Gold;
        var gold = session.Party.Purse.Gold;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal(gold - price, session.Party.Purse.Gold);
        Assert.Contains(session.Inventory.Gear, owned => ReferenceEquals(owned, piece));
        Assert.Equal(RepositoryScreenText.Text.Messages.Format(MessageIds.ShopBought, new MessageArgument("name", name)), shop.Notice);
    }

    [Fact]
    public void Confirm_BoughtPiece_LeavesTheBuyListAndJoinsTheSellList()
    {
        var (shop, _, store) = ShopOnAGearRow();
        var piece = shop.SelectedRow!.Piece!;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.DoesNotContain(store.Stock.Pieces, left => ReferenceEquals(left, piece));
        Assert.NotSame(piece, shop.SelectedRow?.Piece);
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        MoveDownUntil(shop, row => ReferenceEquals(row.Piece, piece));
    }

    [Fact]
    public void Confirm_PieceShortOfGold_ChangesNothing()
    {
        var (shop, session, store) = ShopOnAGearRow(gold: 0);
        var piece = shop.SelectedRow!.Piece!;
        Assert.Equal(TradeRefusal.ShortOfGold, shop.SelectedRow.Refusal);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal(0, session.Party.Purse.Gold);
        Assert.Contains(store.Stock.Pieces, left => ReferenceEquals(left, piece));
        Assert.DoesNotContain(session.Inventory.Gear, owned => ReferenceEquals(owned, piece));
    }

    [Fact]
    public void Notice_ShortOfGold_IsTheReason()
    {
        var (shop, _, _) = ShopOnAGearRow(gold: 0);

        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.ShopRefusedShortOfGold), shop.Notice);
    }

    // The next row is affordable too (the fixture's purse is full), so nothing replaces the cleared notice.
    [Fact]
    public void Down_AfterBuying_ClearsTheNotice()
    {
        var (shop, _, _) = ShopOnAGearRow();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        Assert.NotNull(shop.Notice);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Null(shop.Notice);
    }

    // Buying with the last gold greys the next row; the purchase still shows over its reason until the cursor moves.
    [Fact]
    public void Notice_BoughtWithTheLastGold_StaysTheBoughtLine()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var store = session.ShopFor("general_store");
        var cap = store.Stock.Pieces.First(piece => piece.Base.Id == "leather_cap");
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold));
        session.Party.Purse.Add(store.QuoteBuy(cap).Gold);
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, store);
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        MoveDownUntil(shop, row => ReferenceEquals(row.Piece, cap));
        Assert.True(shop.SelectedRow!.IsAvailable);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal(0, session.Party.Purse.Gold);
        Assert.False(shop.SelectedRow!.IsAvailable);
        Assert.StartsWith("Bought", shop.Notice);
    }

    [Fact]
    public void Right_OnTabsAfterBuying_ClearsTheNotice()
    {
        var (shop, _, _) = ShopOnAGearRow();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Null(shop.Notice);
    }

    // The rows quote their refusal when built, so the purse is set before the Shop opens: full, or empty.
    private static (ShopScene Shop, GameSession Session, Shop Store) ShopOnAGearRow(int gold = Purse.MaxGold)
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold));
        session.Party.Purse.Add(gold);
        var store = session.ShopFor("general_store");
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, store);
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        MoveDownUntil(shop, row => row.Piece is not null);
        return (shop, session, store);
    }

    // Fails rather than hangs when no row matches: Down stops moving at the last row.
    private static void MoveDownUntil(ShopScene shop, Func<TradeRow, bool> wanted)
    {
        while (shop.SelectedRow is not { } row || !wanted(row))
        {
            var before = shop.SelectedRow;
            shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));
            Assert.NotSame(before, shop.SelectedRow);
        }
    }
}
