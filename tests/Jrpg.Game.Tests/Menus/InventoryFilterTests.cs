using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Game.Menus;

namespace Jrpg.Game.Tests.Menus;

public sealed class InventoryFilterTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // Picked up out of category order, so a test can see the order kept.
    private static readonly IReadOnlyList<InventoryEntry> _owned =
    [
        new("leather_armor", 1),
        new("potion", 4),
        new("bronze_sword", 2),
        new("wizard_robe", 1),
        new("iron_sword", 1),
        new("bomb", 3),
        new("scrap", 2),
    ];

    [Fact]
    public void SelectStacks_Weapons_KeepsOnlyWeaponKindItems()
    {
        var kept = InventoryFilter.SelectStacks(InventoryCategory.Weapons, _owned, _catalog.Items);

        Assert.Equal(["bronze_sword", "iron_sword"], kept.Select(entry => entry.ItemId));
    }

    [Fact]
    public void SelectStacks_Consumables_KeepsItemsWithEffect()
    {
        var kept = InventoryFilter.SelectStacks(InventoryCategory.Consumables, _owned, _catalog.Items);

        Assert.Equal(["potion", "bomb"], kept.Select(entry => entry.ItemId));
    }

    [Fact]
    public void SelectStacks_Armor_KeepsEveryPieceThatIsNotAWeapon()
    {
        var kept = InventoryFilter.SelectStacks(InventoryCategory.Armor, _owned, _catalog.Items);

        Assert.Equal(["leather_armor", "wizard_robe"], kept.Select(entry => entry.ItemId));
    }

    [Fact]
    public void SelectStacks_Materials_KeepsOnlyMaterials()
    {
        var kept = InventoryFilter.SelectStacks(InventoryCategory.Materials, _owned, _catalog.Items);

        Assert.Equal(["scrap"], kept.Select(entry => entry.ItemId));
    }

    [Fact]
    public void SelectStacks_All_KeepsPickupOrder()
    {
        var kept = InventoryFilter.SelectStacks(InventoryCategory.All, _owned, _catalog.Items);

        Assert.Equal(_owned, kept);
    }

    [Fact]
    public void SelectPieces_Armor_KeepsEveryPieceButWeapons()
    {
        GearPiece[] gear = [Piece("leather_armor"), Piece("bronze_sword"), Piece("power_ring"), Piece("wooden_shield")];

        var kept = InventoryFilter.SelectPieces(InventoryCategory.Armor, gear);

        Assert.Equal([Piece("leather_armor"), Piece("power_ring"), Piece("wooden_shield")], kept);
    }

    [Fact]
    public void SelectPieces_Consumables_KeepsNone()
    {
        var kept = InventoryFilter.SelectPieces(InventoryCategory.Consumables, [Piece("bronze_sword")]);

        Assert.Empty(kept);
    }

    private static GearPiece Piece(string itemId) => GearPiece.Plain(_catalog.Items[itemId]);
}
