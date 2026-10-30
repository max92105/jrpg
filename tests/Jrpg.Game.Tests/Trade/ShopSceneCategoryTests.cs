using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// The tab buttons change the shown list's category from the list (decision 384); each tab keeps its own.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneCategoryTests
{
    [Fact]
    public void NextTab_InTheList_ChangesTheCategoryAndGoesToTheFirstRow()
    {
        var shop = NewShop();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.NextTab));

        Assert.Equal(InventoryCategory.Consumables, shop.Category);
        Assert.Equal("potion", shop.SelectedRow?.Item.Id);
    }

    [Fact]
    public void PreviousTab_OnAll_WrapsToMaterials()
    {
        var shop = NewShop();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.PreviousTab));

        Assert.Equal(InventoryCategory.Materials, shop.Category);
    }

    [Fact]
    public void NextTab_OnSell_LeavesTheBuyCategory()
    {
        var shop = NewShop();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.NextTab));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Left));

        Assert.Equal(InventoryCategory.All, shop.Category);
    }

    [Fact]
    public void NextTab_OnTheTabs_ChangesNothing()
    {
        var shop = NewShop();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.NextTab));

        Assert.Equal(InventoryCategory.All, shop.Category);
        Assert.True(shop.IsOnTabs);
    }

    private static ShopScene NewShop()
    {
        GameSession session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        return new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
    }
}
