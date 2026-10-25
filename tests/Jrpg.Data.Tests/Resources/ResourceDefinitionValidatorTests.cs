using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Resources;

// Resource rules checked through the class catalog, which names the class and the resource in each message.
public sealed class ResourceDefinitionValidatorTests
{
    [Fact]
    public void Validate_DominanceOverUnknownResource_Throws()
    {
        var growth = Force("growth") with { Dominance = new DominanceRule("decay", 50) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("druid") with { Resources = [growth] }]));

        Assert.Equal("classes.json: class 'druid': resource 'growth': dominance.over must name another resource of the class (was 'decay').", exception.Message);
    }

    [Fact]
    public void Validate_DominanceNotNamedBack_Throws()
    {
        var growth = Force("growth") with { Dominance = new DominanceRule("decay", 50) };
        var decay = Force("decay");

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("druid") with { Resources = [growth, decay] }]));

        Assert.EndsWith("dominance.over names 'decay', whose dominance doesn't name 'growth' back.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_DominanceBonusNegative_Throws()
    {
        var growth = Force("growth") with { Dominance = new DominanceRule("decay", -10) };
        var decay = Force("decay") with { Dominance = new DominanceRule("growth", 50) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("druid") with { Resources = [growth, decay] }]));

        Assert.EndsWith("dominance.bonusPercent must be at least 0 (was -10).", exception.Message, StringComparison.Ordinal);
    }

    private static ResourceDefinition Force(string id) => new(id, id, Max: 10);

    private static void Validate(IReadOnlyList<ClassDefinition> classes) =>
        ClassCatalogValidator.Validate("classes.json", classes, new ClassReferences(new Dictionary<string, SkillDefinition>()));

    private static ClassDefinition Class(string id)
    {
        var stats = new ClassStats(MaxHp: 200, Attack: 20, Defense: 5, Speed: 10);
        return new ClassDefinition(id, "Druid", "Tends.", stats, stats);
    }
}
