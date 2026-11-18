using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// rules.json is written whole with the catalogs (ADR 0061, decision 955), on a copy of the test world: checked as the
// loader checks it (its sections, the Forge's materials against the items, the classes' skill levels against its
// table), written only when it changed since the stamp; a hand edit is two appended bytes.
public sealed class ContentWriterRulesTests : IDisposable
{
    private const string FileName = "rules.json";
    private const string HandEdit = "\r\n";

    private readonly CopiedContentRoot _content = new("writer-rules", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterRulesTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
        _writer.StampCatalogs(_catalog);
    }

    public void Dispose() => _content.Dispose();

    // One more level, so the file is written; every other section must come back as it went.
    [Fact]
    public void WriteMapCatalogs_Rules_LoadTheSameAfterRelay()
    {
        var written = _catalog.Rules with { Experience = new([.. _catalog.Rules.Experience.ExperienceToNextLevel, 3800]) };

        _writer.WriteMapCatalogs(_catalog with { Rules = written });

        var reloaded = ContentCatalogLoader.Load(_content.Path).Rules;
        Assert.Equal(written.Experience.ExperienceToNextLevel, reloaded.Experience.ExperienceToNextLevel);
        Assert.Equal(written.Loot.Rarities.OrderBy(rarity => rarity.Key), reloaded.Loot.Rarities.OrderBy(rarity => rarity.Key));
        Assert.Equal(ByRarity(written.Crafting.Salvage), ByRarity(reloaded.Crafting.Salvage));
        Assert.Equal(ByRarity(written.Crafting.UpgradeCosts), ByRarity(reloaded.Crafting.UpgradeCosts));
        Assert.Equal(written.Crafting.RerollCost, reloaded.Crafting.RerollCost);
        Assert.Equal(written.Party, reloaded.Party);
        Assert.Equal(written.Exploration, reloaded.Exploration);
        Assert.Equal(written.Stats, reloaded.Stats);
        Assert.Equal(written.Trade, reloaded.Trade);
    }

    // The added level shows the text is the writer's, not the copied file's.
    [Fact]
    public void WriteMapCatalogs_RelaidRules_SpellRaritiesAsTheFileDoes()
    {
        var table = new ExperienceTable([.. _catalog.Rules.Experience.ExperienceToNextLevel, 3800]);

        _writer.WriteMapCatalogs(_catalog with { Rules = _catalog.Rules with { Experience = table } });

        var text = File.ReadAllText(_content.PathOf(FileName));
        Assert.Contains("3800", text, StringComparison.Ordinal);
        Assert.Contains("\"common\": {", text, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Common\"", text, StringComparison.Ordinal);
    }

    // The hand edit kept proves the file wasn't rewritten while the shops' was.
    [Fact]
    public void WriteMapCatalogs_UnchangedRules_LeavesTheFile()
    {
        File.AppendAllText(_content.PathOf(FileName), HandEdit);
        var handEdited = File.ReadAllBytes(_content.PathOf(FileName));
        var store = _catalog.Shops["general_store"] with { Name = "Corner Store" };
        var shops = new Dictionary<string, ShopDefinition>(_catalog.Shops) { [store.Id] = store };

        _writer.WriteMapCatalogs(_catalog with { Shops = shops });

        Assert.Equal(handEdited, File.ReadAllBytes(_content.PathOf(FileName)));
        Assert.Equal("Corner Store", ContentCatalogLoader.Load(_content.Path).Shops["general_store"].Name);
    }

    [Fact]
    public void WriteMapCatalogs_EmptyExperienceTable_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));
        var rules = _catalog.Rules with { Experience = new([]) };

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(_catalog with { Rules = rules }));

        Assert.Equal("rules.json: experience.experienceToNextLevel must have at least one entry.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    // The items without the Forge's scrap: the draft's items, not an edit of the rules, refuse the save.
    [Fact]
    public void WriteMapCatalogs_CraftingNamingRemovedItem_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));
        var items = new Dictionary<string, ItemDefinition>(_catalog.Items);
        items.Remove("scrap");

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(_catalog with { Items = items }));

        Assert.Equal("rules.json: crafting.salvage.common[0]: itemId has unknown item id 'scrap'.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    // The test's class alone, so no repository class learning a skill late is named first; expose costs nothing, so
    // only its level is refused. Three entries make level 4 the max.
    [Fact]
    public void WriteMapCatalogs_TableShorterThanASkillLevel_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));
        var ranger = new ClassDefinition(
            "ranger", "Ranger", "A hunter who waits for the shot.", new ClassStats(160, 14), new ClassStats(15, 1))
        {
            Skills = [new("expose") { Level = 5 }],
        };
        var edited = _catalog with
        {
            Classes = new Dictionary<string, ClassDefinition> { [ranger.Id] = ranger },
            Rules = _catalog.Rules with { Experience = new([100, 120, 144]) },
        };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(edited));

        Assert.Equal(
            "classes.json: class 'ranger': skills lists 'expose' at level 5, past the max level 4.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    private static List<(Rarity Rarity, InventoryEntry Entry)> ByRarity(
        IReadOnlyDictionary<Rarity, IReadOnlyList<InventoryEntry>> table) =>
        [.. table.OrderBy(row => row.Key).SelectMany(row => row.Value.Select(entry => (row.Key, entry)))];
}
