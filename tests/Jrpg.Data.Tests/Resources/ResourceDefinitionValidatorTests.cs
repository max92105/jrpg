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

    [Fact]
    public void Validate_StrainFromNonLasting_Throws()
    {
        var clarity = Force("clarity") with { Strain = new StrainRule("mana", 1) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("mage") with { Resources = [Force("mana"), clarity] }]));

        Assert.Equal("classes.json: class 'mage': resource 'clarity': strain is only for a lasting resource.", exception.Message);
    }

    [Fact]
    public void Validate_StrainOnUnknownResource_Throws()
    {
        var clarity = Force("clarity") with { IsLasting = true, Strain = new StrainRule("mana", 1) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("mage") with { Resources = [clarity] }]));

        Assert.EndsWith("strain.lowersStartOf must name another resource of the class (was 'mana').", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_StrainPerPointZero_Throws()
    {
        var clarity = Force("clarity") with { IsLasting = true, Strain = new StrainRule("mana", 0) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("mage") with { Resources = [Force("mana"), clarity] }]));

        Assert.EndsWith("strain.perMissingPoint must be at least 1 (was 0).", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_StrainOnLastingResource_Throws()
    {
        var faith = Force("faith") with { IsLasting = true };
        var clarity = Force("clarity") with { IsLasting = true, Strain = new StrainRule("faith", 1) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("mage") with { Resources = [faith, clarity] }]));

        Assert.EndsWith("strain.lowersStartOf can't name a lasting resource ('faith'), whose lowered start would carry on and drop again.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_LastingFromSpeed_Throws()
    {
        var readiness = new ResourceDefinition("readiness", "Readiness") { Color = TestColor, MaxFromSpeed = new SpeedScaling(10), IsLasting = true };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("rogue") with { Resources = [readiness] }]));

        Assert.EndsWith("a lasting resource can't take its max from speed, which changes with level.", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("#5B8CF")]
    [InlineData("5B8CFFF")]
    [InlineData("#5B8CFG")]
    public void Validate_BadResourceColor_ThrowsNamingClassAndResource(string color)
    {
        var mana = Force("mana") with { Color = color };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("mage") with { Resources = [mana] }]));

        Assert.Equal($"classes.json: class 'mage': resource 'mana': color must be # followed by six hex digits (was '{color}').", exception.Message);
    }

    [Fact]
    public void Validate_MissingResourceColor_Throws()
    {
        var mana = Force("mana") with { Color = "" };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("mage") with { Resources = [mana] }]));

        Assert.EndsWith("color must be # followed by six hex digits (was '').", exception.Message, StringComparison.Ordinal);
    }

    // Any valid color: the other tests are about other fields.
    private const string TestColor = "#5B8CFF";

    private static ResourceDefinition Force(string id) => new(id, id, Max: 10) { Color = TestColor };

    private static void Validate(IReadOnlyList<ClassDefinition> classes) =>
        ClassCatalogValidator.Validate("classes.json", classes, new ClassReferences(new Dictionary<string, SkillDefinition>()));

    private static ClassDefinition Class(string id)
    {
        var stats = new ClassStats(MaxHp: 200, Attack: 20, Defense: 5, Speed: 10);
        return new ClassDefinition(id, "Druid", "Tends.", stats, stats);
    }
}
