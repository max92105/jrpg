using Jrpg.Core.Trade;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The shops are replaced whole as one undo step (decision 591); what an undo would take away is listed so it can be
// refused while a map's event opens it.
public sealed class CatalogDraftShopTests
{
    private static readonly ShopDefinition _armory = new("armory", "Armory", ["bronze_sword"]);

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void SetShops_NewShop_IsOneUndoStep()
    {
        _draft.SetShops([.. _draft.Current.Shops.Values, _armory]);

        Assert.Equal(_armory, _draft.Current.Shops["armory"]);
        Assert.True(_draft.IsDirty);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.False(_draft.Current.Shops.ContainsKey("armory"));
    }

    [Fact]
    public void SetShops_SameShops_RecordsNothing()
    {
        var copies = _draft.Current.Shops.Values.Select(shop => shop with { ItemIds = [.. shop.ItemIds] }).ToList();

        _draft.SetShops(copies);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void ShopsRemovedByUndo_AfterAdding_ListsIt()
    {
        _draft.SetShops([.. _draft.Current.Shops.Values, _armory]);

        Assert.Equal(["armory"], _draft.ShopsRemovedByUndo());
    }

    [Fact]
    public void ShopsRemovedByUndo_AfterRenamingOnly_ListsNone()
    {
        var store = _draft.Current.Shops["general_store"];
        _draft.SetShops([store with { Name = "Corner Store" }]);

        Assert.Empty(_draft.ShopsRemovedByUndo());
    }
}
