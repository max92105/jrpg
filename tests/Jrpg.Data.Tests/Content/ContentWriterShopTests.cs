using System.Text.Json.Nodes;
using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The editor's save path validates through the writer's references, which must know the shops (ADR 0031): without them,
// a map with an openShop step could never be saved. shops.json itself is saved with the other catalogs (ADR 0045),
// checked against the items as the loader checks it, sorted by id.
public sealed class ContentWriterShopTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-shop", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterShopTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMaps_MapWithOpenShopStep_IsAccepted()
    {
        var start = _catalog.Maps["start"];
        var shopCell = new CellEvent(new GridCell(0, 0), new MapEvent { Id = "shop_cell", Activation = EventActivation.Touch, Steps = [new OpenShopStep("general_store")] });
        var withShop = new MapDefinition(start.Layout with { CellEvents = [.. start.Layout.CellEvents, shopCell] }, start.Settings);

        _writer.WriteMaps(MapContent.With(_catalog, withShop), [withShop.Id]);

        var loaded = ContentCatalogLoader.Load(_content.Path).Maps["start"].Layout.CellEvents;
        Assert.Contains(loaded, cell => cell.Event.Steps is [OpenShopStep { ShopId: "general_store" }]);
    }

    [Fact]
    public void WriteMaps_OpenShopStepNamingUnknownShop_ThrowsNamingIt()
    {
        var start = _catalog.Maps["start"];
        var shopCell = new CellEvent(new GridCell(0, 0), new MapEvent { Id = "shop_cell", Activation = EventActivation.Touch, Steps = [new OpenShopStep("bazaar")] });
        var withShop = new MapDefinition(start.Layout with { CellEvents = [.. start.Layout.CellEvents, shopCell] }, start.Settings);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMaps(MapContent.With(_catalog, withShop), [withShop.Id]));

        Assert.Contains("openShop has unknown shop id 'bazaar'.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMapCatalogs_Shops_LoadTheSameAfterRelay()
    {
        _writer.WriteMapCatalogs(_catalog);

        var reloaded = ContentCatalogLoader.Load(_content.Path).Shops;
        Assert.Equal(_catalog.Shops.Keys.Order(StringComparer.Ordinal), reloaded.Keys.Order(StringComparer.Ordinal));
        Assert.All(_catalog.Shops.Values, shop => Assert.Equal(shop, reloaded[shop.Id]));
    }

    [Fact]
    public void WriteMapCatalogs_ShopWithUnknownItem_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf("shops.json"));
        var broken = WithShop(new ShopDefinition("armory", "Armory", ["elixir_of_nothing"]));

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(broken));

        Assert.Equal("shops.json: shop 'armory': itemIds[0] has unknown item id 'elixir_of_nothing'.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf("shops.json")));
    }

    // The new shop sorts before the general store; the other shops are the repository's to change.
    [Fact]
    public void WriteMapCatalogs_NewShop_WritesItSorted()
    {
        var armory = new ShopDefinition("armory", "Armory", ["bronze_sword", "potion"]);

        _writer.WriteMapCatalogs(WithShop(armory));

        var written = JsonNode.Parse(File.ReadAllText(_content.PathOf("shops.json")))!.AsArray();
        var ids = written.Select(shop => (string?)shop!["id"]).ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.True(ids.IndexOf("armory") < ids.IndexOf("general_store"));
        Assert.Equal(armory, ContentCatalogLoader.Load(_content.Path).Shops["armory"]);
    }

    private ContentCatalog WithShop(ShopDefinition shop) =>
        _catalog with { Shops = new Dictionary<string, ShopDefinition>(_catalog.Shops) { [shop.Id] = shop } };
}
