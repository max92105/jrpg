using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageTests
{
    [Fact]
    public void Update_NextCategory_ResetsCursorToFirstRow()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
        Assert.Equal(2, page.SelectedRow);

        page.Update(Pressing(GameButton.NextTab));

        Assert.Equal(InventoryCategory.Consumables, page.Category);
        Assert.Equal(0, page.SelectedRow);
    }

    [Fact]
    public void Constructor_StartingItems_ShowsAllInPickupOrder()
    {
        var session = NewSession();

        var page = NewPage(session);

        Assert.Equal(session.Inventory.Entries.Select(entry => entry.ItemId), page.ShownItemIds);
    }

    [Fact]
    public void Update_PreviousCategoryFromAll_WrapsToArmor()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.PreviousTab));

        Assert.Equal(InventoryCategory.Armor, page.Category);
        Assert.Equal(["leather_armor", "chain_mail", "wizard_robe"], page.ShownItemIds);
    }

    [Fact]
    public void Update_CategoryWithNothingOwned_HasNoSelectedRow()
    {
        var session = NewSession();
        foreach (var armor in new[] { "leather_armor", "chain_mail", "wizard_robe" })
        {
            session.Inventory.TryRemove(armor, session.Inventory.QuantityOf(armor));
        }

        var page = NewPage(session);
        page.Update(Pressing(GameButton.PreviousTab));
        page.Update(Pressing(GameButton.Down));

        Assert.Equal(InventoryCategory.Armor, page.Category);
        Assert.Empty(page.ShownItemIds);
        Assert.Null(page.SelectedRow);
    }

    private static GameSession NewSession() => GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));

    private static InventoryPage NewPage(GameSession? session = null) =>
        new(RepositoryScreenText.Text, new UiTextures(), session ?? NewSession(), StartMapScenarios.BelowClosedDoor);
}
