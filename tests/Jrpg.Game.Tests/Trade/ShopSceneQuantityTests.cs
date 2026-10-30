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

// Confirm on an available stack opens the quantity picker at 1; while it is open it takes every press (decision 377),
// and its Confirm buys the count at Core's total (decision 379).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneQuantityTests
{
    [Fact]
    public void Confirm_Stack_OpensThePickerAtOne()
    {
        var (shop, _, _) = ShopOnAStackRow();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal(1, shop.Picker!.Count);
        Assert.False(shop.IsOnTabs);
    }

    [Fact]
    public void ConfirmInPicker_BuysTheCountAndTakesTheTotal()
    {
        var (shop, session, store) = ShopOnAStackRow();
        var item = shop.SelectedRow!.Item;
        var name = shop.SelectedRow.Name.Text;
        var owned = session.Inventory.QuantityOf(item.Id);
        var gold = session.Party.Purse.Gold;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        var total = store.QuoteBuy(item, 3).Gold;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(shop.Picker);
        Assert.Equal(gold - total, session.Party.Purse.Gold);
        Assert.Equal(owned + 3, session.Inventory.QuantityOf(item.Id));
        var messages = RepositoryScreenText.Text.Messages;
        Assert.Equal(messages.Format(MessageIds.ShopBoughtCount, new MessageArgument("name", name), MessageArgument.Number("count", 3)), shop.Notice);
    }

    [Fact]
    public void Cancel_InPicker_ClosesItOnly()
    {
        var (shop, session, _) = ShopOnAStackRow();
        var row = shop.SelectedRow;
        var gold = session.Party.Purse.Gold;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.Null(shop.Picker);
        Assert.False(shop.IsOnTabs);
        Assert.Same(row, shop.SelectedRow);
        Assert.Equal(gold, session.Party.Purse.Gold);
    }

    [Fact]
    public void Right_InPicker_StepsTheCountNotTheMember()
    {
        var (shop, session, _) = ShopOnAStackRow();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Equal(11, shop.Picker!.Count);
        Assert.Same(session.Party.Leader, shop.PreviewedMember);
    }

    [Fact]
    public void Left_InPicker_StepsTheCountDownNotTheMember()
    {
        var (shop, session, _) = ShopOnAStackRow();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Left));

        Assert.Equal(1, shop.Picker!.Count);
        Assert.Same(session.Party.Leader, shop.PreviewedMember);
    }

    [Fact]
    public void Down_InPicker_WrapsToTheMostNotTheList()
    {
        var (shop, _, store) = ShopOnAStackRow();
        var row = shop.SelectedRow!;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Equal(store.MostBuyable(row.Item), shop.Picker!.Count);
        Assert.Same(row, shop.SelectedRow);
    }

    // The purse is full, so the stock's first stack is available and its most is the room left in the bag.
    private static (ShopScene Shop, GameSession Session, Shop Store) ShopOnAStackRow()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        Assert.True(session.Party.TryAddToActive(session.Party.ReserveMembers[0]));
        session.Party.Purse.Add(Purse.MaxGold);
        var store = session.ShopFor("general_store");
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, store);
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        Assert.True(shop.SelectedRow is { Piece: null, IsAvailable: true });
        return (shop, session, store);
    }
}
