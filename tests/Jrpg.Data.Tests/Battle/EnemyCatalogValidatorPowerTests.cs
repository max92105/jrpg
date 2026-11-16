using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Battle;

// An enemy's Power (decision 787): within 0..10 000, and at least 1 once its tactics or a phase name a tiered skill.
// Over a goblin and two skills built here, Cleave tiered and Jab flat.
public sealed class EnemyCatalogValidatorPowerTests
{
    private static readonly EnemyDefinition _goblin = new()
    {
        Id = "goblin",
        Name = "Goblin",
        PortraitFile = "goblin.png",
        BattleSpriteFile = "Goblin.png",
        AttackEffectFile = "AttackEnemy.png",
        IsSpriteFlipped = false,
        MaxHp = 40,
        Damage = 20,
        DamageType = DamageType.Piercing,
        Accuracy = 85,
        Dodge = 5,
        CritChance = 5,
        CrowdControlResistance = 10,
        ExperienceReward = 40,
    };

    private static readonly EnemyReferences _references = new(
        new HashSet<string>(),
        new Dictionary<string, SkillDefinition>
        {
            ["cleave"] = Skill("cleave", new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Slashing)
            {
                Tier = new TieredEffect("slashing_1", EffectKind.Damage, 2) { DamageType = DamageType.Slashing },
            }),
            ["jab"] = Skill("jab", new Effect(EffectKind.Damage, 8, Targeting.SingleEnemy, DamageType: DamageType.Piercing)),
        });

    private static readonly EnemyAction[] _cleaves = [new EnemyAction { SkillId = "cleave", Weight = 1 }];

    public static TheoryData<EnemyDefinition, string> InvalidPowers => new()
    {
        { _goblin with { Power = -1 }, "power must be within 0..10000 (was -1)." },
        { _goblin with { Power = 10_001 }, "power must be within 0..10000 (was 10001)." },
        { _goblin with { Tactics = _cleaves }, "power must be at least 1: its skill 'cleave' names a tiered effect (was 0)." },
        {
            _goblin with { Phases = [new EnemyPhase { BelowHpPercent = 50, Tactics = _cleaves }] },
            "power must be at least 1: its skill 'cleave' names a tiered effect (was 0)."
        },
    };

    [Theory]
    [MemberData(nameof(InvalidPowers))]
    public void Validate_InvalidPower_ThrowsNamingTheEnemy(EnemyDefinition enemy, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => EnemyCatalogValidator.Validate("enemies.json", [enemy], _references));

        Assert.Equal($"enemies.json: enemy 'goblin': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_TieredSkillWithPowerOne_Accepted()
    {
        Assert.Null(Record.Exception(
            () => EnemyCatalogValidator.Validate("enemies.json", [_goblin with { Power = 1, Tactics = _cleaves }], _references)));
    }

    [Fact]
    public void Validate_PowerTenThousand_Accepted()
    {
        Assert.Null(Record.Exception(
            () => EnemyCatalogValidator.Validate("enemies.json", [_goblin with { Power = 10_000 }], _references)));
    }

    // A flat skill needs no Power.
    [Fact]
    public void Validate_PowerZeroWithoutTieredSkills_Accepted()
    {
        var jabber = _goblin with { Tactics = [new EnemyAction { SkillId = "jab", Weight = 1 }] };

        Assert.Null(Record.Exception(() => EnemyCatalogValidator.Validate("enemies.json", [jabber], _references)));
    }

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", ResourceChanges = [], Effect = effect };
}
