using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The rules' problem line (decision 955): the loader's own message for the edited rules, its sections' rules first,
// then the classes' skill levels against its table, then the Forge's materials against the content's items.
public sealed class CatalogProblemsRulesTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void OfRules_ValidRules_IsNull() => Assert.Null(CatalogProblems.OfRules(_catalog.Rules, _catalog));

    [Fact]
    public void OfRules_ZeroExperienceEntry_NamesIt()
    {
        var rules = _catalog.Rules with { Experience = new([100, 0, 144]) };

        var problem = CatalogProblems.OfRules(rules, _catalog);

        Assert.Equal("rules.json: experience.experienceToNextLevel[1] must be at least 1 (was 0).", problem);
    }

    // The test's class alone, so no repository class learning a skill late is named first; three entries make level 4
    // the max level.
    [Fact]
    public void OfRules_TableShorterThanASkillLevel_NamesTheClass()
    {
        var ranger = new ClassDefinition(
            "ranger", "Ranger", "A hunter who waits for the shot.", new ClassStats(160, 14), new ClassStats(15, 1))
        {
            Skills = [new("expose") { Level = 5 }],
        };
        var content = _catalog with { Classes = new Dictionary<string, ClassDefinition> { [ranger.Id] = ranger } };

        var problem = CatalogProblems.OfRules(_catalog.Rules with { Experience = new([100, 120, 144]) }, content);

        Assert.Equal("classes.json: class 'ranger': skills lists 'expose' at level 5, past the max level 4.", problem);
    }

    [Fact]
    public void OfRules_CraftingNamingRemovedItem_NamesIt()
    {
        var items = new Dictionary<string, ItemDefinition>(_catalog.Items);
        items.Remove("scrap");

        var problem = CatalogProblems.OfRules(_catalog.Rules, _catalog with { Items = items });

        Assert.Equal("rules.json: crafting.salvage.common[0]: itemId has unknown item id 'scrap'.", problem);
    }
}
