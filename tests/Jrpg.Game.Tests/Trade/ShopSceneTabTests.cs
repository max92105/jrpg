using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// The Shop opens on its tabs: Left and Right choose without wrapping, Confirm enters the shown list, and Cancel in the
// list returns to the tabs (the plan's planner's picks).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneTabTests
{
    [Fact]
    public void Update_Opened_CursorOnTheBuyTab()
    {
        var shop = NewShop();

        Assert.True(shop.IsOnTabs);
        Assert.Equal(ShopTab.Buy, shop.Tab);
    }

    [Fact]
    public void Right_OnBuy_ShowsSell()
    {
        var shop = NewShop();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Equal(ShopTab.Sell, shop.Tab);
        Assert.True(shop.IsOnTabs);
    }

    [Fact]
    public void Left_OnBuy_StaysOnBuy()
    {
        var shop = NewShop();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Left));

        Assert.Equal(ShopTab.Buy, shop.Tab);
    }

    [Fact]
    public void Right_OnSell_StaysOnSell()
    {
        var shop = NewShop();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Equal(ShopTab.Sell, shop.Tab);
    }

    [Fact]
    public void Confirm_OnTabs_EntersTheList()
    {
        var shop = NewShop();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.False(shop.IsOnTabs);
        Assert.NotNull(shop.SelectedRow);
    }

    // Cancel wins when nested: the Shop stays open, and a later Cancel on the tabs is what closes it.
    [Fact]
    public void Cancel_InTheList_ReturnsToTheTabs()
    {
        var shop = NewShop();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var request = shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        var later = Enumerable.Range(0, ShopSceneTests.ClosingFadeUpdates).Select(_ => shop.Update(StartMapScenarios.OneUpdate, default)).ToList();

        Assert.Null(request);
        Assert.True(shop.IsOnTabs);
        Assert.All(later, Assert.Null);
    }

    [Fact]
    public void Down_InTheSellList_MovesItsOwnCursor()
    {
        var shop = NewShop();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        var first = shop.SelectedRow;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.NotNull(first);
        Assert.NotEqual(first, shop.SelectedRow);
    }

    // Each tab keeps its own cursor: moving in the Sell list leaves the Buy list on its first row.
    [Fact]
    public void Down_InTheSellList_LeavesTheBuyListsCursor()
    {
        var shop = NewShop();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        var buyFirst = shop.SelectedRow;
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Left));

        Assert.Equal(buyFirst, shop.SelectedRow);
    }

    private static ShopScene NewShop()
    {
        GameSession session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        return new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
    }
}
