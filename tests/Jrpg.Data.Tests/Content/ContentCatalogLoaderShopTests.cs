using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// shops.json is checked against the items once those are loaded: a copy of the content with a shop naming an unpriced
// item proves the loader makes that check.
public sealed class ContentCatalogLoaderShopTests : IDisposable
{
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-shops-{Guid.NewGuid():N}");

    public ContentCatalogLoaderShopTests()
    {
        var source = Path.Combine(AppContext.BaseDirectory, "content");
        foreach (var file in Directory.EnumerateFiles(source, "*.json", SearchOption.AllDirectories))
        {
            var target = Path.Combine(_contentRoot, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    [Fact]
    public void Load_Repository_GeneralStoreListsPricedItems()
    {
        var catalog = ContentCatalogLoader.Load(_contentRoot);

        var store = catalog.Shops["general_store"];
        Assert.Contains("potion", store.ItemIds);
        Assert.All(store.ItemIds, id => Assert.NotNull(catalog.Items[id].Price));
        Assert.InRange(catalog.Rules.Trade.SellPercent, 1, 100);
    }

    [Fact]
    public void Load_ShopNamingUnpricedItem_ThrowsNamingTheFile()
    {
        var shopsPath = Path.Combine(_contentRoot, "shops.json");
        File.WriteAllText(shopsPath, File.ReadAllText(shopsPath).Replace("\"potion\"", "\"scrap\"", StringComparison.Ordinal));

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_contentRoot));

        Assert.StartsWith("shops.json", exception.Message, StringComparison.Ordinal);
        Assert.EndsWith("itemIds[0] 'scrap' has no price, so no shop can sell it.", exception.Message, StringComparison.Ordinal);
    }
}
