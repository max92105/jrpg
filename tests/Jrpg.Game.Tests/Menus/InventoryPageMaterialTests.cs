using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// Materials list under their own tab and All, drawn in their color, and are never used (decision 339).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageMaterialTests
{
    [Fact]
    public void Show_ScrapOwned_RowIsMaterial()
    {
        var session = SessionWithScrap();
        var list = new InventoryList(session, StartMapScenarios.BelowClosedDoor, RepositoryScreenText.Text.Fonts.FontAt(26), RepositoryScreenText.Text.Messages);

        list.Show(InventoryCategory.All, selectedRow: 0);

        Assert.True(list.Rows.Single(row => row.ItemId == "scrap").IsMaterial);
        Assert.False(list.Rows.First(row => row.ItemId == "potion").IsMaterial);
    }

    [Fact]
    public void Update_MaterialsTab_ListsScrapButNoPotion()
    {
        var page = NewPage(SessionWithScrap());

        InventoryPageTabs.Show(page, InventoryCategory.Materials);

        Assert.Contains("scrap", page.ShownItemIds);
        Assert.DoesNotContain("potion", page.ShownItemIds);
    }

    [Fact]
    public void Update_ConfirmOnMaterial_DoesNothing()
    {
        var session = SessionWithScrap();
        var page = NewPage(session);
        InventoryPageTabs.Show(page, InventoryCategory.Materials);

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.IsChoosingTarget);
        Assert.Null(page.Notice);
        Assert.Equal(3, session.Inventory.QuantityOf("scrap"));
    }

    private static GameSession SessionWithScrap()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        session.Inventory.Add(StartMapScenarios.BelowClosedDoor.Items["scrap"], 3);
        return session;
    }

    private static InventoryPage NewPage(GameSession session) =>
        new(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
}
