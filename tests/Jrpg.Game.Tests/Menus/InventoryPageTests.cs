using Jrpg.Core.Items;
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
    public void Open_AfterLastCopyTakenElsewhere_NoLongerListsIt()
    {
        var session = NewSession();
        var page = NewPage(session);
        var taken = session.Inventory.Entries[0];
        session.Inventory.TryRemove(taken.ItemId, taken.Quantity);

        page.Open();

        Assert.DoesNotContain(taken.ItemId, page.ShownItemIds);
    }

    [Fact]
    public void Open_AfterChangingCategory_ShowsAll()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.NextTab));

        page.Open();

        Assert.Equal(InventoryCategory.All, page.Category);
        Assert.Equal(0, page.SelectedRow);
    }

    [Fact]
    public void Constructor_StartingItems_ShowsStacksThenGearInPickupOrder()
    {
        var session = NewSession();

        var page = NewPage(session);

        string[] expected = [.. session.Inventory.Entries.Select(entry => entry.ItemId), .. session.Inventory.Gear.Select(piece => piece.Base.Id)];
        Assert.Equal(expected, page.ShownItemIds);
    }

    [Fact]
    public void Update_PreviousCategoryFromAll_WrapsToMaterials()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.PreviousTab));

        Assert.Equal(InventoryCategory.Materials, page.Category);
    }

    [Fact]
    public void Update_ArmorCategory_ListsArmorButNoWeapon()
    {
        var page = NewPage();

        InventoryPageTabs.Show(page, InventoryCategory.Armor);

        Assert.Equal(["leather_armor", "chain_mail", "wizard_robe"], page.ShownItemIds.Distinct().Take(3));
        Assert.DoesNotContain("bronze_sword", page.ShownItemIds);
        Assert.Contains("wooden_shield", page.ShownItemIds);
    }

    [Fact]
    public void Update_CategoryWithNothingOwned_HasNoSelectedRow()
    {
        var session = NewSession();
        foreach (var armor in session.Inventory.Gear.Where(piece => piece.Kind != EquipmentKind.Weapon).ToList())
        {
            session.Inventory.TryRemoveGear(armor);
        }

        var page = NewPage(session);
        InventoryPageTabs.Show(page, InventoryCategory.Armor);
        page.Update(Pressing(GameButton.Down));

        Assert.Equal(InventoryCategory.Armor, page.Category);
        Assert.Empty(page.ShownItemIds);
        Assert.Null(page.SelectedRow);
    }

    [Fact]
    public void Update_LeftThenDown_ShowsConsumables()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Down));

        Assert.Equal(InventoryCategory.Consumables, page.Category);
    }

    [Fact]
    public void Update_LeftFromList_PutsTheCursorOnTheRail()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Left));

        Assert.True(page.IsChoosingCategory);
    }

    [Fact]
    public void Update_RightOnRail_ReturnsToTheListOnItsFirstRow()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Left));

        page.Update(Pressing(GameButton.Right));

        Assert.False(page.IsChoosingCategory);
        Assert.Equal(0, page.SelectedRow);
    }

    // The list starts on the potion, which Confirm on the list would use.
    [Fact]
    public void Update_ConfirmOnRail_ReturnsToTheListAndUsesNothing()
    {
        var session = NewSession();
        var page = NewPage(session);
        page.Update(Pressing(GameButton.Left));

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.IsChoosingCategory);
        Assert.False(page.IsChoosingTarget);
        Assert.Equal(5, session.Inventory.QuantityOf("potion"));
    }

    [Fact]
    public void Update_UpOnRailFromAll_WrapsToMaterials()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Left));

        page.Update(Pressing(GameButton.Up));

        Assert.Equal(InventoryCategory.Materials, page.Category);
    }

    [Fact]
    public void Update_NextTabOnRail_ChangesCategoryAndStaysOnRail()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Left));

        page.Update(Pressing(GameButton.NextTab));

        Assert.Equal(InventoryCategory.Consumables, page.Category);
        Assert.True(page.IsChoosingCategory);
    }

    [Fact]
    public void LeavesOnCancel_OnRail_IsTrue()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Left));

        Assert.True(page.LeavesOnCancel);
    }

    [Fact]
    public void Hint_AfterLeft_IsTheCategoryHint()
    {
        var page = NewPage();
        Assert.Equal(Hint(MessageIds.MenuHintInventory), page.Hint);

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(Hint(MessageIds.MenuHintInventoryCategories), page.Hint);
    }

    [Fact]
    public void Hint_BackOnTheList_IsTheListHint()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Left));
        Assert.Equal(Hint(MessageIds.MenuHintInventoryCategories), page.Hint);

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(Hint(MessageIds.MenuHintInventory), page.Hint);
    }

    [Fact]
    public void Open_AfterLeavingFromTheRail_StartsOnTheList()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Left));

        page.Open();

        Assert.False(page.IsChoosingCategory);
    }

    private static string Hint(string messageId) => RepositoryScreenText.Text.Messages.Text(messageId);

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static InventoryPage NewPage(GameSession? session = null) =>
        new(RepositoryScreenText.Text, new UiTextures(), session ?? NewSession(), StartMapScenarios.BelowClosedDoor);
}
