using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A skill's or item's own strip is read from its file (decision 678), on a copy of the test world given the field after
// the record's id, which never changes.
public sealed class ContentCatalogLoaderEffectFileTests
{
    [Fact]
    public void Load_SkillNamingAStrip_ReadsIt()
    {
        using var content = new CopiedContentRoot("skill-strip", "test-world");
        content.Replace("skills.json", "\"id\": \"fire_blast\",", "\"id\": \"fire_blast\", \"effectFile\": \"Burst.png\",");

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal("Burst.png", catalog.Skills["fire_blast"].EffectFile);
    }

    [Fact]
    public void Load_ItemNamingAStrip_ReadsIt()
    {
        using var content = new CopiedContentRoot("item-strip", "test-world");
        content.Replace("items.json", "\"id\": \"bomb\",", "\"id\": \"bomb\", \"effectFile\": \"Burst.png\",");

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal("Burst.png", catalog.Items["bomb"].EffectFile);
    }

    [Fact]
    public void Load_SkillWithoutAStrip_HasNone()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

        Assert.Null(catalog.Skills["heal"].EffectFile);
    }
}
