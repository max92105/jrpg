using Jrpg.Core.Trade;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Tests.Trade;

// The Shops page starts from the draft and its whole session is one undo step (decision 591).
public sealed class ShopEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly ShopEditingViewModel _shopEditing;

    public ShopEditingViewModelTests() => _shopEditing = new ShopEditingViewModel(_draft);

    [Fact]
    public void Start_Draft_ListsItsShops()
    {
        var edited = _shopEditing.Start();

        Assert.Contains(edited.Shops, shop => shop.Id == "general_store" && !shop.IsNew);
    }

    [Fact]
    public void Apply_AddedShop_IsOneUndoStep()
    {
        var edited = _shopEditing.Start();
        edited.Shops.Single(shop => shop.Id == "general_store").Name = "Corner Store";
        edited.AddShopCommand.Execute(null);
        edited.SelectedShop!.Id = "smithy";
        edited.SelectedShop.Name = "Smithy";
        edited.SelectedShop.ItemToAdd = edited.OfferedItems.Single(item => item.Id == "bronze_sword");
        edited.SelectedShop.AddItemCommand.Execute(null);

        _shopEditing.Apply(edited);

        Assert.Equal("Corner Store", _draft.Current.Shops["general_store"].Name);
        Assert.Equal(new ShopDefinition("smithy", "Smithy", ["bronze_sword"]), _draft.Current.Shops["smithy"]);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.False(_draft.Current.Shops.ContainsKey("smithy"));
    }

    [Fact]
    public void Apply_Unchanged_RecordsNothing()
    {
        _shopEditing.Apply(_shopEditing.Start());

        Assert.False(_draft.CanUndo);
    }
}
