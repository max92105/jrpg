using Jrpg.Game.Menus;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

public sealed class InventoryListViewTests
{
    // A material is never usable: its color must win over the grey of what the menu can't use.
    [Fact]
    public void TextColorOf_UnusableMaterial_IsTheMaterialColor()
    {
        var row = new InventoryRow("scrap", "Scrap", "Item.png", Quantity: null, "Metal scraps.", IsUsable: false) { IsMaterial = true };

        Assert.Equal(UiTheme.RarityMaterial, InventoryListView.TextColorOf(row));
    }

    [Fact]
    public void TextColorOf_UnusablePlainItem_IsGreyed()
    {
        var row = new InventoryRow("leather_armor", "Leather Armor", "Armor.png", Quantity: null, "Armor.", IsUsable: false);

        Assert.Equal(UiTheme.TextDisabled, InventoryListView.TextColorOf(row));
    }
}
