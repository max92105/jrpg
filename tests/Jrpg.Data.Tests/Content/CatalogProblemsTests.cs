using Jrpg.Core.Content;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// The loader's own message for an edited catalog, for the editor's problem lines; null when it would load.
/// </summary>
public sealed class CatalogProblemsTests
{
    private static readonly string _testWorld = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_testWorld);

    [Fact]
    public void OfFieldSprites_TestWorldSheets_IsNull() =>
        Assert.Null(CatalogProblems.OfFieldSprites([.. _catalog.FieldSprites.Values], _catalog));

    [Fact]
    public void OfShops_TestWorldShops_IsNull() => Assert.Null(CatalogProblems.OfShops([.. _catalog.Shops.Values], _catalog));

    [Fact]
    public void OfShops_EmptyName_NamesShopAndField()
    {
        var shops = WithGeneralStore(store => store with { Name = " " });

        Assert.Equal("shops.json: shop 'general_store': name must not be empty.", CatalogProblems.OfShops(shops, _catalog));
    }

    [Fact]
    public void OfShops_UnknownItem_NamesIt()
    {
        var shops = WithGeneralStore(store => store with { ItemIds = ["elixir_of_nothing"] });

        var problem = CatalogProblems.OfShops(shops, _catalog);

        Assert.Equal("shops.json: shop 'general_store': itemIds[0] has unknown item id 'elixir_of_nothing'.", problem);
    }

    [Fact]
    public void OfFieldSprites_ScaleZero_NamesTheFileAndTheField()
    {
        var sheets = _catalog.FieldSprites.Values
            .Select(sheet => sheet.File == "player.png" ? sheet with { Scale = 0 } : sheet)
            .ToList();

        var problem = CatalogProblems.OfFieldSprites(sheets, _catalog);

        Assert.Equal("field-sprites.json: field sprite 'player.png': scale must be within 1..8 (was 0).", problem);
    }

    [Fact]
    public void OfFieldSprites_ObjectNamingADroppedSheet_NamesTheObject()
    {
        var withoutPlaceholder = _catalog.FieldSprites.Values.Where(sheet => sheet.File != "npc_placeholder.png").ToList();

        var problem = CatalogProblems.OfFieldSprites(withoutPlaceholder, _catalog);

        Assert.Equal(
            "field-sprites.json: object 'villager' names field sprite 'npc_placeholder.png', which has no record.", problem);
    }

    [Fact]
    public void OfFieldSprites_ObjectWithoutSpriteDroppedSheet_IsNull()
    {
        var tileObjects = _catalog.Objects.Values
            .Where(definition => definition.FieldSpriteFile is null)
            .ToDictionary(definition => definition.Id);
        var content = _catalog with { Objects = tileObjects, NewGame = _catalog.NewGame with { FieldSpriteFile = "player.png" } };

        Assert.Null(CatalogProblems.OfFieldSprites([_catalog.FieldSprites["player.png"]], content));
    }

    private static List<ShopDefinition> WithGeneralStore(Func<ShopDefinition, ShopDefinition> change) =>
        [.. _catalog.Shops.Values.Select(shop => shop.Id == "general_store" ? change(shop) : shop)];
}
