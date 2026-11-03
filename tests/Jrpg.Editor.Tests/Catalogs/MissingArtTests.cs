using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

public sealed class MissingArtTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void IsSheetMissing_ImageAbsent_IsTrue()
    {
        var missing = MissingArt.Of(_catalog, file => file != "City.png", _ => true);

        Assert.True(missing.IsSheetMissing("city"));
    }

    [Fact]
    public void IsSheetMissing_ImagePresent_IsFalse()
    {
        var missing = MissingArt.Of(_catalog, _ => true, _ => true);

        Assert.False(missing.IsSheetMissing("city"));
    }

    [Fact]
    public void IsSpriteMissing_NpcSpriteAbsent_IsTrue()
    {
        var missing = MissingArt.Of(_catalog, _ => true, _ => false);

        Assert.True(missing.IsSpriteMissing("npc_placeholder.png"));
    }
}
