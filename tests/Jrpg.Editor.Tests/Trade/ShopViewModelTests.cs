using Jrpg.Core.Trade;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Tests.Trade;

// One shop as the Shops window edits it (ADR 0045): a saved id is fixed, an item is added after the chosen one and
// never twice, and the problem is only the typed id's.
public sealed class ShopViewModelTests
{
    private static readonly ShopDefinition _store = new("general_store", "General Store", ["potion", "bomb"]);
    private static readonly PricedItemChoice _campKit = new("camp_kit", "Camp Kit", 60);

    [Fact]
    public void From_Shop_ReadsNameAndItemsInOrder()
    {
        var shop = ShopViewModel.From(_store);

        Assert.False(shop.IsNew);
        Assert.Equal(("general_store", "General Store"), (shop.Id, shop.Name));
        Assert.Equal(["potion", "bomb"], shop.ItemIds);
        Assert.Equal("potion", shop.SelectedItem);
        Assert.Equal("general_store: General Store", shop.Label);
    }

    [Fact]
    public void AddItem_ChosenItem_AppendsAfterSelected()
    {
        var shop = ShopViewModel.From(_store);
        shop.ItemToAdd = _campKit;

        shop.AddItemCommand.Execute(null);

        Assert.Equal(["potion", "camp_kit", "bomb"], shop.ItemIds);
        Assert.Equal("camp_kit", shop.SelectedItem);
    }

    [Fact]
    public void AddItem_AlreadyListed_IsGreyed()
    {
        var shop = ShopViewModel.From(_store);

        shop.ItemToAdd = new PricedItemChoice("bomb", "Bomb", 40);

        Assert.False(shop.AddItemCommand.CanExecute(null));
    }

    [Fact]
    public void AddItem_NothingPicked_IsGreyed()
    {
        var shop = ShopViewModel.From(_store);

        Assert.False(shop.AddItemCommand.CanExecute(null));
    }

    // Adding the picked item greys Add at once: the same item can't go in twice.
    [Fact]
    public void AddItem_AfterAdding_GreysUntilAnotherIsPicked()
    {
        var shop = ShopViewModel.From(_store);
        shop.ItemToAdd = _campKit;
        var lit = new List<bool>();
        shop.AddItemCommand.CanExecuteChanged += (_, _) => lit.Add(shop.AddItemCommand.CanExecute(null));

        shop.AddItemCommand.Execute(null);

        Assert.Contains(false, lit);
        Assert.False(shop.AddItemCommand.CanExecute(null));
        shop.ItemToAdd = new PricedItemChoice("ether", "Ether", 30);
        Assert.True(shop.AddItemCommand.CanExecute(null));
    }

    [Fact]
    public void MoveItemUp_SecondItem_SwapsWithFirst()
    {
        var shop = ShopViewModel.From(_store);
        shop.SelectedItem = "bomb";

        shop.MoveItemUpCommand.Execute(null);

        Assert.Equal(["bomb", "potion"], shop.ItemIds);
    }

    [Fact]
    public void RemoveItem_First_RemovesItAndSelectsTheNext()
    {
        var shop = ShopViewModel.From(_store);

        shop.RemoveItemCommand.Execute(null);

        Assert.Equal(["bomb"], shop.ItemIds);
        Assert.Equal("bomb", shop.SelectedItem);
    }

    [Fact]
    public void Problem_NewShopWithoutId_AsksForIt()
    {
        var shop = ShopViewModel.New();

        Assert.True(shop.IsNew);
        Assert.Equal("The id must not be empty.", shop.Problem);
        Assert.Equal("(new shop)", shop.Label);
        Assert.Throws<InvalidOperationException>(shop.ToShop);
    }

    [Fact]
    public void Problem_NewShopWithUpperCaseId_NamesTheRule()
    {
        var shop = ShopViewModel.New();

        shop.Id = "Smithy";

        Assert.Equal("The id may only use lowercase letters, digits and _ (like tree_4).", shop.Problem);
    }

    // A saved id is fixed, whatever it looks like: the hand-written content's ids were never checked by this rule.
    [Fact]
    public void Problem_SavedShop_IdNotChecked()
    {
        var shop = ShopViewModel.From(_store with { Id = "General-Store" });

        Assert.Null(shop.Problem);
    }

    [Fact]
    public void ToShop_TypedShop_TrimsTheName()
    {
        var shop = ShopViewModel.New();
        shop.Id = "smithy";
        shop.Name = " Smithy ";
        shop.ItemToAdd = _campKit;
        shop.AddItemCommand.Execute(null);

        Assert.Equal(new ShopDefinition("smithy", "Smithy", ["camp_kit"]), shop.ToShop());
    }
}
