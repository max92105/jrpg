using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The editor's save path validates through the writer's references, which must know the shops (ADR 0031): without them,
// a map with an openShop step could never be saved.
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
}
