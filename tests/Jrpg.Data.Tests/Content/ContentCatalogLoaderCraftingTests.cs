using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// rules.json is read before the items, so its crafting block is checked only once they are loaded: a copy of the
// content with a broken block proves the loader makes that check.
public sealed class ContentCatalogLoaderCraftingTests : IDisposable
{
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-crafting-{Guid.NewGuid():N}");

    public ContentCatalogLoaderCraftingTests()
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
    public void Load_SalvageYieldsAPotion_ThrowsNamingRulesJson()
    {
        var rulesPath = Path.Combine(_contentRoot, "rules.json");
        var rules = File.ReadAllText(rulesPath);
        File.WriteAllText(rulesPath, rules.Replace("\"common\": [{ \"itemId\": \"scrap\"", "\"common\": [{ \"itemId\": \"potion\"", StringComparison.Ordinal));

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_contentRoot));

        Assert.Equal("rules.json: crafting.salvage.common[0]: itemId 'potion' must be a material (isMaterial true).", exception.Message);
    }
}
