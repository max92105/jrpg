using Jrpg.Core.Items;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// The Items page's list (decision 877): the items by category then id, the added ones after, Add by category and
// Duplicate keeping it (decision 878), only an added item removable, and the problem line ending with Data's message
// for everything that reads the items (decision 863).
public sealed class ItemCatalogViewModelTests
{
    [Fact]
    public void Items_Constructed_SortedByCategoryThenId()
    {
        ItemDefinition[] reversed =
            [ItemFixtures.Ore, ItemFixtures.WarlordAxe, ItemFixtures.CopperVest, ItemFixtures.CopperAxe, ItemFixtures.Tonic];
        var content = ItemFixtures.Catalog with { Items = reversed.ToDictionary(item => item.Id, StringComparer.Ordinal) };

        var catalog = new ItemCatalogViewModel(content, []);

        Assert.Equal(["tonic", "copper_axe", "copper_vest", "warlord_axe", "ore"], catalog.Items.Select(item => item.Id));
    }

    [Fact]
    public void Problem_Constructed_IsNull()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void AddItem_Equipment_AddsANewArmorLastAndChoosesIt()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);

        catalog.AddItemCommand.Execute(ItemCategory.Equipment);

        var added = catalog.Items[^1];
        Assert.Same(added, catalog.SelectedItem);
        Assert.Equal((true, ItemCategory.Equipment, "Armor.png"), (added.IsNew, added.Category, added.IconFile));
        Assert.Equal(EquipmentKind.Chest, Typed(added, "tin_vest").Equipment?.Kind);
    }

    [Fact]
    public void AddItem_Unique_AddsANewUniqueWithNoFixedAffix()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);

        catalog.AddItemCommand.Execute(ItemCategory.Unique);

        var unique = Typed(catalog.Items[^1], "test_band");
        Assert.True(unique.IsUnique);
        Assert.Empty(unique.FixedAffixes);
        Assert.Equal(ItemCategory.Unique, catalog.SelectedItem?.Category);
    }

    [Fact]
    public void Duplicate_ChosenUnique_AddsANewUniqueCopyLast()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.SelectedItem = ItemOf(catalog, "warlord_axe");

        catalog.DuplicateCommand.Execute(null);

        var copy = catalog.Items[^1];
        Assert.Same(copy, catalog.SelectedItem);
        Assert.Equal((true, ItemCategory.Unique, "Warlord's Axe"), (copy.IsNew, copy.Category, copy.Name));
        Assert.Equal(ItemFixtures.WarlordAxe.FixedAffixes, Typed(copy, "warlord_axe_2").FixedAffixes);
    }

    [Fact]
    public void Duplicate_NothingChosen_CannotExecute()
    {
        var noItems = ItemFixtures.Catalog with { Items = new Dictionary<string, ItemDefinition>() };
        var catalog = new ItemCatalogViewModel(noItems, []);

        Assert.False(catalog.DuplicateCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveItem_SavedItemChosen_CannotExecute()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.SelectedItem = ItemOf(catalog, "tonic");

        Assert.False(catalog.RemoveItemCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveItem_NewItem_RemovesItAndChoosesItsNeighbor()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.AddItemCommand.Execute(ItemCategory.Consumable);
        var removed = catalog.SelectedItem!;
        catalog.AddItemCommand.Execute(ItemCategory.Material);
        var neighbor = catalog.SelectedItem;
        catalog.SelectedItem = removed;

        catalog.RemoveItemCommand.Execute(null);

        Assert.DoesNotContain(removed, catalog.Items);
        Assert.Same(neighbor, catalog.SelectedItem);
    }

    [Fact]
    public void Problem_TwoNewItemsWithTheSameId_NamesIt()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        AddMaterial(catalog, "pebble");

        AddMaterial(catalog, "pebble");

        Assert.Equal("Item pebble is listed twice.", catalog.Problem);
    }

    // Set up against list order: the earlier item's problem would come first if the chosen one's weren't read first.
    [Fact]
    public void Problem_ChosenItemHasAProblem_IsItsOwnFirst()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        AddMaterial(catalog, "pebble").PriceText = "abc";

        AddMaterial(catalog, "flint").PriceText = "xyz";

        Assert.Equal("Price must be a whole number or empty (was 'xyz').", catalog.Problem);
    }

    [Fact]
    public void Problem_PriceClearedOnAnItemAShopSells_IsDatasShopsMessage()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.SelectedItem = ItemOf(catalog, "potion");

        catalog.SelectedItem.PriceText = "";

        Assert.Equal(
            "shops.json: shop 'general_store': itemIds[0] 'potion' has no price, so no shop can sell it.", catalog.Problem);
    }

    [Fact]
    public void Problem_WeaponDamageTypedZero_IsDatasMessage()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.SelectedItem = ItemOf(catalog, "copper_axe");

        catalog.SelectedItem.Equipment!.DamageText = "0";

        Assert.Equal(
            "items.json: item 'copper_axe': equipment.damage must be at least 1 for a weapon (was 0).", catalog.Problem);
    }

    // Data requires a weapon's type, which is why the Damage type box offers no "none".
    [Fact]
    public void Problem_ArmorTurnedWeaponWithoutAType_IsDatasMessage()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.SelectedItem = ItemOf(catalog, "copper_vest");

        catalog.SelectedItem.Equipment!.Kind = EquipmentKind.Weapon;
        catalog.SelectedItem.Equipment.DamageText = "5";

        Assert.Equal("items.json: item 'copper_vest': equipment.damageType is required for a weapon.", catalog.Problem);
    }

    [Fact]
    public void Problem_WeaponDamageTypedOne_IsNull()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.SelectedItem = ItemOf(catalog, "copper_axe");

        catalog.SelectedItem.Equipment!.DamageText = "1";

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void Problem_EquipmentDamageTyped_IsRaised()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.SelectedItem = ItemOf(catalog, "copper_axe");
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        catalog.SelectedItem.Equipment!.DamageText = "12";

        Assert.Contains(nameof(ItemCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void Problem_TypedIdChanged_IsRaised()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.AddItemCommand.Execute(ItemCategory.Material);
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        catalog.SelectedItem!.Id = "pebble";

        Assert.Contains(nameof(ItemCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void Problem_OtherItemChosen_IsRaised()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        catalog.SelectedItem = ItemOf(catalog, "ore");

        Assert.Contains(nameof(ItemCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void ToItems_WithAProblem_Throws()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.AddItemCommand.Execute(ItemCategory.Consumable);

        Assert.Throws<InvalidOperationException>(catalog.ToItems);
    }

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);

        catalog.Choose("ore");

        Assert.Equal("ore", catalog.SelectedItem?.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);
        catalog.Choose("ore");

        catalog.Choose("missing");

        Assert.Equal("ore", catalog.SelectedItem?.Id);
    }

    // The host shows a record by its key, and a new item has none until its id is typed.
    [Fact]
    public void ChosenKey_NewItemWithoutId_IsNull()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, []);

        catalog.AddItemCommand.Execute(ItemCategory.Material);

        Assert.Null(catalog.ChosenKey);
    }

    private static ItemViewModel ItemOf(ItemCatalogViewModel catalog, string id) => catalog.Items.Single(item => item.Id == id);

    private static ItemViewModel AddMaterial(ItemCatalogViewModel catalog, string id)
    {
        catalog.AddItemCommand.Execute(ItemCategory.Material);
        var added = catalog.SelectedItem!;
        added.Id = id;
        added.Name = "Stone";
        return added;
    }

    // A new item's record once its id is typed, the one box its problem reads before Data's.
    private static ItemDefinition Typed(ItemViewModel item, string id)
    {
        item.Id = id;
        return item.ToItem();
    }
}
