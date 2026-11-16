using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Tests.Trade;

// The Shops page's list (ADR 0045): the draft's shops in id order, only a new shop removable (decision 581), the
// picker's priced items by name (decision 584), and the problem line ending with Data's message (decision 580).
public sealed class ShopCatalogViewModelTests
{
    private static readonly ShopDefinition _store = new("general_store", "General Store", ["potion", "bomb"]);
    private static readonly ShopDefinition _armory = new("armory", "Armory", ["bomb"]);

    // The tests' own items and shops over the test world: a priced potion and bomb, an heirloom nobody sells.
    private static readonly ContentCatalog _content =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")) with
        {
            Items = new Dictionary<string, ItemDefinition>
            {
                ["potion"] = Item("potion", "Potion", 20),
                ["bomb"] = Item("bomb", "Bomb", 40),
                ["heirloom"] = Item("heirloom", "Heirloom", price: null),
            },
            Shops = new Dictionary<string, ShopDefinition> { ["general_store"] = _store, ["armory"] = _armory },
        };

    [Fact]
    public void Shops_Draft_InIdOrderWithTheFirstChosen()
    {
        var catalog = new ShopCatalogViewModel(_content);

        Assert.Equal(["armory", "general_store"], catalog.Shops.Select(shop => shop.Id));
        Assert.Same(catalog.Shops[0], catalog.SelectedShop);
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void OfferedItems_OnlyPricedOnes_SortedByNameThenId()
    {
        var catalog = new ShopCatalogViewModel(_content);

        PricedItemChoice[] byName = [new("bomb", "Bomb", 40), new("potion", "Potion", 20)];
        Assert.Equal(byName, catalog.OfferedItems);
        Assert.Equal("Bomb (bomb), 40 g", catalog.OfferedItems[0].Label);
    }

    [Fact]
    public void RemoveShop_SavedShop_IsGreyed()
    {
        var catalog = new ShopCatalogViewModel(_content);

        Assert.False(catalog.RemoveShopCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveShop_NewShop_RemovesItAndSelectsAnother()
    {
        var catalog = new ShopCatalogViewModel(_content);
        catalog.AddShopCommand.Execute(null);
        Assert.True(catalog.RemoveShopCommand.CanExecute(null));

        catalog.RemoveShopCommand.Execute(null);

        Assert.Equal(["armory", "general_store"], catalog.Shops.Select(shop => shop.Id));
        Assert.Same(catalog.Shops[1], catalog.SelectedShop);
        Assert.Null(catalog.Problem);
    }

    // A removed shop is no longer watched: typing in it (a stale binding) raises nothing.
    [Fact]
    public void RemoveShop_RemovedShopEdited_RaisesNothing()
    {
        var catalog = new ShopCatalogViewModel(_content);
        catalog.AddShopCommand.Execute(null);
        var removed = catalog.SelectedShop!;
        catalog.RemoveShopCommand.Execute(null);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        removed.Name = "Gone";
        removed.ItemIds.Add("potion");

        Assert.Empty(notified);
    }

    [Fact]
    public void AddShop_SelectsANewShopWithAnEditableId()
    {
        var catalog = new ShopCatalogViewModel(_content);

        catalog.AddShopCommand.Execute(null);

        Assert.Same(catalog.Shops[2], catalog.SelectedShop);
        Assert.True(catalog.SelectedShop!.IsNew);
        Assert.Equal("Shop #3: The id must not be empty.", catalog.Problem);
    }

    [Fact]
    public void Problem_EmptyName_IsDataMessageAndNotifies()
    {
        var catalog = new ShopCatalogViewModel(_content);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Shops[1].Name = " ";

        Assert.Equal("shops.json: shop 'general_store': name must not be empty.", catalog.Problem);
        Assert.Contains(nameof(ShopCatalogViewModel.Problem), notified);
    }

    [Fact]
    public void Problem_LastItemRemoved_IsDataMessageAndNotifies()
    {
        var catalog = new ShopCatalogViewModel(_content);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Shops[0].RemoveItemCommand.Execute(null);

        Assert.Equal("shops.json: shop 'armory': itemIds must list at least one item.", catalog.Problem);
        Assert.Contains(nameof(ShopCatalogViewModel.Problem), notified);
    }

    [Fact]
    public void Problem_TwoNewShopsSameId_SaysSo()
    {
        var catalog = new ShopCatalogViewModel(_content);
        catalog.AddShopCommand.Execute(null);
        catalog.SelectedShop!.Id = "smithy";
        catalog.AddShopCommand.Execute(null);

        catalog.SelectedShop!.Id = "smithy";

        Assert.Equal("Shop smithy is listed twice.", catalog.Problem);
    }

    [Fact]
    public void Problem_NewShopWithASavedId_SaysSo()
    {
        var catalog = new ShopCatalogViewModel(_content);
        catalog.AddShopCommand.Execute(null);

        catalog.SelectedShop!.Id = "armory";

        Assert.Equal("Shop armory is listed twice.", catalog.Problem);
    }

    [Fact]
    public void ToShops_Unchanged_EqualsDraft()
    {
        var catalog = new ShopCatalogViewModel(_content);

        Assert.Equal([_armory, _store], catalog.ToShops());
    }

    [Fact]
    public void ToShops_WithAProblem_Throws()
    {
        var catalog = new ShopCatalogViewModel(_content);
        catalog.AddShopCommand.Execute(null);

        Assert.Throws<InvalidOperationException>(catalog.ToShops);
    }

    // The armory is chosen first (id order), so only Choose picks the general store.
    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = new ShopCatalogViewModel(_content);

        catalog.Choose("general_store");

        Assert.Equal("general_store", catalog.SelectedShop!.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = new ShopCatalogViewModel(_content);
        catalog.Choose("general_store");

        catalog.Choose("smithy");

        Assert.Equal("general_store", catalog.SelectedShop!.Id);
    }

    [Fact]
    public void ChosenKey_NewShopTyped_IsItsTypedId()
    {
        var catalog = new ShopCatalogViewModel(_content);
        catalog.AddShopCommand.Execute(null);

        catalog.SelectedShop!.Id = "smithy";

        Assert.Equal("smithy", catalog.ChosenKey);
    }

    private static ItemDefinition Item(string id, string name, int? price) => new()
    {
        Id = id,
        Name = name,
        Description = name,
        IconFile = "Placeholder.png",
        MaxStack = 9,
        IsUsableFromMenu = false,
        Price = price,
    };
}
