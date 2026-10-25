using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
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
            Effect = new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly),
        },
        ["heal_all"] = new()
        {
            Id = "heal_all",
            Name = "Heal All",
            Description = "",
            IconFile = "",
            Requirements = [new ResourceRequirement("faith", 6)],
            Effect = new Effect(EffectKind.RestoreHp, 50, Targeting.AllAllies),
        },
        ["smite"] = new()
        {
            Id = "smite",
            Name = "Smite",
            Description = "",
            IconFile = "",
            ResourceChanges = [new ResourceChange("conviction", -4)],
            Effect = new Effect(EffectKind.Damage, 30, Targeting.SingleEnemy),
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
        { Class("fighter") with { Base = Stats with { Attack = -2 } }, "base.attack must be at least 0 (was -2)." },
        { Class("fighter") with { Growth = Stats with { Defense = -1 } }, "growth.defense must be at least 0 (was -1)." },
        { Class("fighter") with { Growth = Stats with { MaxHp = -5 } }, "growth.maxHp must be at least 0 (was -5)." },
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

    [Fact]
    public void Validate_StartsAtAboveMax_ThrowsNamingClassAndResource()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("mage") with { Resources = [Mana with { StartsAt = 11 }] }]));

        Assert.Equal("classes.json: class 'mage': resource 'mana': startsAt must be within 0..10 (was 11).", exception.Message);
    }

    [Fact]
    public void Validate_DuplicateResourceId_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("mage") with { Resources = [Mana, Mana with { Name = "More mana" }] }]));

        Assert.Equal("classes.json: class 'mage': resources lists 'mana' more than once.", exception.Message);
    }

    [Fact]
    public void Validate_MaxBelowOne_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("mage") with { Resources = [Mana with { Max = 0, StartsAt = 0 }] }]));

        Assert.Equal("classes.json: class 'mage': resource 'mana': max must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_EmptyResourceName_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("mage") with { Resources = [Mana with { Name = " " }] }]));

        Assert.Equal("classes.json: class 'mage': resource 'mana': name must not be empty.", exception.Message);
    }

    public static TheoryData<ResourceDefinition, string> MalformedResources => new()
    {
        { null!, "resources must not contain a null entry." },
        { Mana with { Id = " " }, "resources must not contain an empty id." },
    };

    [Theory]
    [MemberData(nameof(MalformedResources))]
    public void Validate_NullOrEmptyResourceId_Throws(ResourceDefinition resource, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("mage") with { Resources = [resource] }]));

        Assert.Equal($"classes.json: class 'mage': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_SkillChangesForeignResource_ThrowsNamingClassSkillAndResource()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("mage") with { SkillIds = ["smite"], Resources = [Mana] }]));

        Assert.Equal(
            "classes.json: class 'mage': skillIds lists 'smite', which uses 'conviction', a resource mage doesn't have.",
            exception.Message);
    }

    [Fact]
    public void Validate_RequirementAboveMax_Throws()
    {
        var faith = new ResourceDefinition("faith", "Faith", Max: 5, StartsAt: 5) { Color = TestColor };

        var exception = Assert.Throws<ContentValidationException>(
            () => Validate([Class("cleric") with { SkillIds = ["heal_all"], Resources = [faith] }]));

        Assert.Equal("classes.json: class 'cleric': skillIds lists 'heal_all', which needs 6 faith, above its max of 5.", exception.Message);
    }

    [Fact]
    public void Validate_HeavyHitGainWithoutPercent_Throws()
    {
        var conviction = Mana with { Id = "conviction", Gains = new ResourceGains { PerHeavyHit = 2 } };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("paladin") with { Resources = [conviction] }]));

        Assert.Equal(
            "classes.json: class 'paladin': resource 'conviction': gains.heavyHitPercent must be within 1..100 when perHeavyHit is set (was 0).",
            exception.Message);
    }

    [Fact]
    public void Validate_ResourceWithMaxAndMaxFromSpeed_Throws()
    {
        var readiness = Mana with { Id = "readiness", MaxFromSpeed = new SpeedScaling(SpeedPerCharge: 10) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("rogue") with { Resources = [readiness] }]));

        Assert.Equal("classes.json: class 'rogue': resource 'readiness': set either max and startsAt, or maxFromSpeed, not both.", exception.Message);
    }

    [Fact]
    public void Validate_SpeedPerChargeBelowOne_Throws()
    {
        var readiness = new ResourceDefinition("readiness", "Readiness") { Color = TestColor, MaxFromSpeed = new SpeedScaling(0) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("rogue") with { Resources = [readiness] }]));

        Assert.Equal("classes.json: class 'rogue': resource 'readiness': maxFromSpeed.speedPerCharge must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeSpeed_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("rogue") with { Growth = Stats with { Speed = -1 } }]));

        Assert.Equal("classes.json: class 'rogue': growth.speed must be at least 0 (was -1).", exception.Message);
    }

    [Fact]
    public void Validate_BackfireChanceAbove100_Throws()
    {
        var instability = Mana with { Id = "instability", Backfire = new BackfireRule(ChancePerPointPercent: 150, DamagePercentOfMaxHp: 10) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("warlock") with { Resources = [instability] }]));

        Assert.Equal("classes.json: class 'warlock': resource 'instability': backfire percents must be within 1..100 (were 150 and 10).", exception.Message);
    }

    [Fact]
    public void Validate_BackfireDamageZero_Throws()
    {
        var instability = Mana with { Id = "instability", Backfire = new BackfireRule(ChancePerPointPercent: 5, DamagePercentOfMaxHp: 0) };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("warlock") with { Resources = [instability] }]));

        Assert.EndsWith("backfire percents must be within 1..100 (were 5 and 0).", exception.Message, StringComparison.Ordinal);
    }

    // A speed-scaled resource still goes through the rule checks after its amounts.
    [Fact]
    public void Validate_SpeedScaledResourceWithBadHeavyHitPercent_Throws()
    {
        var readiness = new ResourceDefinition("readiness", "Readiness")
        {
            Color = TestColor,
            MaxFromSpeed = new SpeedScaling(10),
            Gains = new ResourceGains { PerHeavyHit = 1, HeavyHitPercent = 0 },
        };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([Class("rogue") with { Resources = [readiness] }]));

        Assert.EndsWith("gains.heavyHitPercent must be within 1..100 when perHeavyHit is set (was 0).", exception.Message, StringComparison.Ordinal);
    }

    // Any valid color: these tests are about other fields.
    private const string TestColor = "#5B8CFF";

    private static ResourceDefinition Mana { get; } = new("mana", "Mana", Max: 10, StartsAt: 3) { Color = TestColor };

    private static void Validate(IReadOnlyList<ClassDefinition> classes) =>
        ClassCatalogValidator.Validate("classes.json", classes, _references);

    private static ClassDefinition Class(string id) => new(id, "Fighter", "Fights.", Stats, Stats);
}
