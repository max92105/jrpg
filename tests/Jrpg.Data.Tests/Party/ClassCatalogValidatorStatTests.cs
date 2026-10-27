using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

// A class's base stats and growth per level.
public sealed class ClassCatalogValidatorStatTests
{
    private static readonly ClassReferences _noSkills = new(new Dictionary<string, SkillDefinition>());

    public static TheoryData<ClassDefinition, string> InvalidStats => new()
    {
        { Class("fighter") with { Base = Stats with { MaxHp = 0 } }, "base.maxHp must be at least 1 (was 0)." },
        { Class("fighter") with { Base = Stats with { Attack = -2 } }, "base.attack must be at least 0 (was -2)." },
        { Class("fighter") with { Growth = Stats with { Defense = -1 } }, "growth.defense must be at least 0 (was -1)." },
        { Class("fighter") with { Growth = Stats with { MaxHp = -5 } }, "growth.maxHp must be at least 0 (was -5)." },
        { Class("fighter") with { Base = Stats with { Accuracy = -1 } }, "base.accuracy must be at least 0 (was -1)." },
        { Class("fighter") with { Base = Stats with { Primaries = new PrimaryStats(1, -1, 1, 1, 1) } }, "base.primaries.agility must be at least 0 (was -1)." },
        { Class("fighter") with { Base = Stats with { Primaries = new PrimaryStats(1, 1, -1, 1, 1) } }, "base.primaries.intelligence must be at least 0 (was -1)." },
        { Class("fighter") with { Growth = Stats with { Primaries = new PrimaryStats(1, 1, 1, -1, 1) } }, "growth.primaries.constitution must be at least 0 (was -1)." },
        { Class("fighter") with { Growth = Stats with { Primaries = new PrimaryStats(1, 1, 1, 1, -1) } }, "growth.primaries.will must be at least 0 (was -1)." },
    };

    private static ClassStats Stats { get; } = new(MaxHp: 200, Attack: 20, Defense: 5, Speed: 10);

    [Theory]
    [MemberData(nameof(InvalidStats))]
    public void Validate_InvalidStat_ThrowsNamingTheClassAndField(ClassDefinition classDefinition, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate([classDefinition]));

        Assert.Equal($"classes.json: class 'fighter': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_ZeroGrowth_DoesNotThrow()
    {
        var neverImproving = Class("fighter") with { Growth = new ClassStats(MaxHp: 0, Attack: 0, Defense: 0, Speed: 0) };

        var exception = Record.Exception(() => Validate([neverImproving]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NegativeBasePrimary_NamesTheField()
    {
        var classDefinition = Class("fighter") with { Base = Stats with { Primaries = new PrimaryStats(-1, 8, 6, 14, 8) } };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([classDefinition]));

        Assert.Equal("classes.json: class 'fighter': base.primaries.power must be at least 0 (was -1).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeGrowthAccuracy_NamesTheField()
    {
        var classDefinition = Class("fighter") with { Growth = Stats with { Accuracy = -2 } };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([classDefinition]));

        Assert.Equal("classes.json: class 'fighter': growth.accuracy must be at least 0 (was -2).", exception.Message);
    }

    private static void Validate(IReadOnlyList<ClassDefinition> classes) =>
        ClassCatalogValidator.Validate("classes.json", classes, _noSkills);

    private static ClassDefinition Class(string id) => new(id, "Fighter", "Fights.", Stats, Stats);
}
