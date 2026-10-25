using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

public sealed class ClassCatalogValidatorTests
{
    private static readonly ClassReferences _references = new(new Dictionary<string, SkillDefinition>
    {
        ["heal"] = new()
        {
            Id = "heal",
            Name = "Heal",
            Description = "",
            IconFile = "",
            MpCost = 8,
            Effect = new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly),
        },
    });

    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        ClassDefinition[] classes = [Class("fighter") with { Name = "Fighter" }, Class("fighter") with { Name = "Berserker" }];

        var exception = Assert.Throws<ContentValidationException>(() => Validate(classes));

        Assert.Equal("classes.json: class id 'fighter' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyName_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("fighter") with { Name = "" }]));

        Assert.Equal("classes.json: class 'fighter': name must not be empty.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyDescription_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("fighter") with { Description = " " }]));

        Assert.Equal("classes.json: class 'fighter': description must not be empty.", exception.Message);
    }

    public static TheoryData<ClassDefinition, string> InvalidStats => new()
    {
        { Class("fighter") with { Base = Stats with { MaxHp = 0 } }, "base.maxHp must be at least 1 (was 0)." },
        { Class("fighter") with { Base = Stats with { MaxMp = -1 } }, "base.maxMp must be at least 0 (was -1)." },
        { Class("fighter") with { Base = Stats with { Attack = -2 } }, "base.attack must be at least 0 (was -2)." },
        { Class("fighter") with { Growth = Stats with { Defense = -1 } }, "growth.defense must be at least 0 (was -1)." },
        { Class("fighter") with { Growth = Stats with { MaxHp = -5 } }, "growth.maxHp must be at least 0 (was -5)." },
    };

    private static ClassStats Stats { get; } = new(MaxHp: 200, MaxMp: 20, Attack: 20, Defense: 5);

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
        var neverImproving = Class("fighter") with { Growth = new ClassStats(MaxHp: 0, MaxMp: 0, Attack: 0, Defense: 0) };

        var exception = Record.Exception(() => Validate([neverImproving]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_UnknownSkillId_ThrowsNamingClassAndSkill()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("cleric") with { SkillIds = ["heal", "meteor"] }]));

        Assert.Equal("classes.json: class 'cleric': skillIds has unknown skill id 'meteor'.", exception.Message);
    }

    [Fact]
    public void Validate_DuplicateSkillId_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("cleric") with { SkillIds = ["heal", "heal"] }]));

        Assert.Equal("classes.json: class 'cleric': skillIds lists 'heal' more than once.", exception.Message);
    }

    [Fact]
    public void Validate_EmptySkillId_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("cleric") with { SkillIds = ["heal", null!] }]));

        Assert.Equal("classes.json: class 'cleric': skillIds must not contain an empty id.", exception.Message);
    }

    [Fact]
    public void Validate_NoSkillIds_Passes()
    {
        var exception = Record.Exception(() => Validate([Class("fighter")]));

        Assert.Null(exception);
    }

    private static void Validate(IReadOnlyList<ClassDefinition> classes) =>
        ClassCatalogValidator.Validate("classes.json", classes, _references);

    private static ClassDefinition Class(string id) => new(id, "Fighter", "Fights.", Stats, Stats);
}
