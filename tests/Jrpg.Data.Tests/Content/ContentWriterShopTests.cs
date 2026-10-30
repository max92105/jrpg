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
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-writer-shop-{Guid.NewGuid():N}");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterShopTests()
    {
        var source = Path.Combine(AppContext.BaseDirectory, "content");
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(_contentRoot, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        _catalog = ContentCatalogLoader.Load(_contentRoot);
        _writer = new ContentWriter(_contentRoot);
    }

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    [Fact]
    public void WriteMap_MapWithOpenShopStep_IsAccepted()
    {
        var start = _catalog.Maps["start"];
        var shopCell = new CellEvent(new GridCell(0, 0), new MapEvent { Id = "shop_cell", Activation = EventActivation.Touch, Steps = [new OpenShopStep("general_store")] });
        var withShop = new MapDefinition(start.Layout with { CellEvents = [.. start.Layout.CellEvents, shopCell] }, start.Settings);

        _writer.WriteMap(withShop, _catalog);

        var loaded = ContentCatalogLoader.Load(_contentRoot).Maps["start"].Layout.CellEvents;
        Assert.Contains(loaded, cell => cell.Event.Steps is [OpenShopStep { ShopId: "general_store" }]);
    }

    [Fact]
    public void WriteMap_OpenShopStepNamingUnknownShop_ThrowsNamingIt()
    {
        var start = _catalog.Maps["start"];
        var shopCell = new CellEvent(new GridCell(0, 0), new MapEvent { Id = "shop_cell", Activation = EventActivation.Touch, Steps = [new OpenShopStep("bazaar")] });
        var withShop = new MapDefinition(start.Layout with { CellEvents = [.. start.Layout.CellEvents, shopCell] }, start.Settings);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMap(withShop, _catalog));

        Assert.Contains("openShop has unknown shop id 'bazaar'.", exception.Message, StringComparison.Ordinal);
    }
}
