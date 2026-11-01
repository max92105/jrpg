using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Content;

namespace Jrpg.Data.Tests.Battle;

// An enemy's actions against the skills (decisions 411, 412): the message names the enemy, the field and the index.
public sealed class EnemyTacticsValidatorTests
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
        new[]
        {
            Skill("cleave", Targeting.AllEnemies),
            Skill("fire_blast", Targeting.SingleEnemy) with { ResourceChanges = [new ResourceChange("mana", -2)] },
            Skill("heal", Targeting.SingleAlly) with { Requirements = [new ResourceRequirement("faith", 1)] },
            Skill("aura", Targeting.AlliesInRange),
            Skill("ready", Targeting.OtherCombatant),
        }.ToDictionary(skill => skill.Id));

    [Fact]
    public void Validate_TacticsWithANullEntry_NamesTheIndex()
    {
        var tactics = new EnemyAction[] { new() { Weight = 1 }, null! };

        Assert.Equal("enemies.json: enemy 'goblin': tactics[1] must not be null.", ProblemOf(tactics));
    }

    [Fact]
    public void Validate_ActionWeightZero_IsRefused()
    {
        Assert.Equal("enemies.json: enemy 'goblin': tactics[0].weight must be at least 1 (was 0).", ProblemOf(new EnemyAction { Weight = 0 }));
    }

    [Fact]
    public void Validate_UnknownSkillId_NamesIt()
    {
        Assert.Equal("enemies.json: enemy 'goblin': tactics[0].skillId has unknown skill id 'slash'.", ProblemOf(Using("slash")));
    }

    [Theory]
    [InlineData("fire_blast")]
    [InlineData("heal")]
    public void Validate_SkillThatChangesOrRequiresAResource_IsRefused(string skillId)
    {
        Assert.Equal($"enemies.json: enemy 'goblin': tactics[0].skillId '{skillId}' uses a class resource, and enemies have none.", ProblemOf(Using(skillId)));
    }

    [Fact]
    public void Validate_SkillAimedAlongTheFormation_IsRefused()
    {
        Assert.Equal("enemies.json: enemy 'goblin': tactics[0].skillId 'aura' aims along the party's formation, which enemies have none of.", ProblemOf(Using("aura")));
    }

    [Fact]
    public void Validate_SkillAimedAtAnyOtherCombatant_IsRefused()
    {
        Assert.Equal("enemies.json: enemy 'goblin': tactics[0].skillId 'ready' places a readied turn, a rogue's resource mechanic.", ProblemOf(Using("ready")));
    }

    [Fact]
    public void Validate_ActionWithoutSkillId_IsAccepted()
    {
        Assert.Null(ProblemOf(new EnemyAction { Weight = 1 }));
    }

    [Fact]
    public void Validate_SkillWithoutAResourceAtWeightOne_IsAccepted()
    {
        Assert.Null(ProblemOf(Using("cleave")));
    }

    // The own skill goes in front of the first one, so the load does not lean on what the repository's skills cost.
    [Fact]
    public void Load_EnemyWithTactics_ReadsTheActions()
    {
        using var content = new CopiedContentRoot("enemy-tactics");
        content.Replace(
            "skills.json",
            "\"id\": \"fire_blast\"",
            "\"id\": \"test_strike\", \"name\": \"Strike\", \"description\": \"\", \"iconFile\": \"Attack.png\", \"effect\": { \"kind\": \"damage\", \"damageType\": \"blunt\", \"power\": 5, \"targeting\": \"singleEnemy\" } }, { \"id\": \"fire_blast\"");
        content.Replace(
            "enemies.json",
            "\"id\": \"goblin\",",
            "\"id\": \"goblin\", \"tactics\": [ { \"weight\": 4 }, { \"skillId\": \"test_strike\", \"weight\": 2 } ],");

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal(
            [new EnemyAction { Weight = 4 }, new EnemyAction { SkillId = "test_strike", Weight = 2 }],
            catalog.Enemies["goblin"].Tactics);
    }

    [Theory]
    [InlineData(TacticConditionKind.OwnHpBelow, 0)]
    [InlineData(TacticConditionKind.OwnHpBelow, 100)]
    [InlineData(TacticConditionKind.AllyHpBelow, 0)]
    [InlineData(TacticConditionKind.AllyHpBelow, 100)]
    public void Validate_HpConditionPercentOutOfRange_IsRefused(TacticConditionKind kind, int percent)
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(kind, percent) };

        Assert.Equal($"enemies.json: enemy 'goblin': tactics[0].when.percent must be within 1..99 (was {percent}).", ProblemOf(action));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Validate_HpConditionPercentAtABound_IsAccepted(int percent)
    {
        Assert.Null(ProblemOf(new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.OwnHpBelow, percent) }));
    }

    [Fact]
    public void Validate_AllyFallenWithAPercent_IsRefused()
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.AllyFallen, 30) };

        Assert.Equal("enemies.json: enemy 'goblin': tactics[0].when.percent must be left out of allyFallen (was 30).", ProblemOf(action));
    }

    [Fact]
    public void Validate_AllyFallenWithoutAPercent_IsAccepted()
    {
        Assert.Null(ProblemOf(new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.AllyFallen) }));
    }

    [Fact]
    public void Load_ActionWithACondition_ReadsTheConditionKind()
    {
        using var content = new CopiedContentRoot("enemy-condition");
        content.Replace(
            "enemies.json",
            "\"id\": \"goblin\",",
            "\"id\": \"goblin\", \"tactics\": [ { \"weight\": 1, \"when\": { \"kind\": \"allyHpBelow\", \"percent\": 50 } } ],");

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal(new TacticCondition(TacticConditionKind.AllyHpBelow, 50), catalog.Enemies["goblin"].Tactics[0].When);
    }

    private static string? ProblemOf(params EnemyAction[] tactics) =>
        Record.Exception(() => EnemyCatalogValidator.Validate("enemies.json", [_goblin with { Tactics = tactics }], _references))?.Message;

    private static EnemyAction Using(string skillId) => new() { SkillId = skillId, Weight = 1 };

    private static SkillDefinition Skill(string id, Targeting targeting) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 5, targeting),
    };
}
