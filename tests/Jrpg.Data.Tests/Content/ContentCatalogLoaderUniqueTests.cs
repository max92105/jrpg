using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// affixes.json loads before items.json, and a unique's fixed affixes take their stats from it as they are read (ADR
// 0058, decision 865), on a copy of the repository's content with the placeholder unique edited here where named.
public sealed class ContentCatalogLoaderUniqueTests : IDisposable
{
    private const string CritChanceLine = "\"affixId\": \"crit_chance\"";

    private readonly CopiedContentRoot _content = new("uniques", "content");

    public void Dispose() => _content.Dispose();

    // Ids and shapes only: the values are the developer's to tune.
    [Fact]
    public void Load_RepositoryPlaceholderUnique_HoldsFixedAffixesWithTheirStats()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);

        var cleaver = catalog.Items["chiefs_cleaver"];
        Assert.True(cleaver.IsUnique);
        Assert.NotEmpty(cleaver.FixedAffixes);
        Assert.All(cleaver.FixedAffixes, affix => Assert.Equal(catalog.Affixes[affix.AffixId].Stat, affix.Stat));
    }

    [Fact]
    public void Load_UniqueNamingAnUnknownAffix_RefusesNamingFileAndId()
    {
        _content.Replace("items.json", CritChanceLine, "\"affixId\": \"spite\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("items.json at $[", exception.Message, StringComparison.Ordinal);
        Assert.EndsWith("].fixedAffixes[1]: no affix 'spite'.", exception.Message, StringComparison.Ordinal);
    }

    // The strict reader lets a null entry through to the item's rules, which name it.
    [Fact]
    public void Load_UniqueWithANullFixedAffix_RefusesNamingTheEntry()
    {
        _content.Replace("items.json", "\"fixedAffixes\": [\r\n", "\"fixedAffixes\": [\r\n      null,\r\n");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("items.json: item 'chiefs_cleaver': fixedAffixes[0] is null.", exception.Message);
    }

    [Fact]
    public void Load_AffixesFileMissing_RefusesNamingIt()
    {
        File.Delete(_content.PathOf("affixes.json"));

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("affixes.json: the file is missing.", exception.Message);
    }
}
