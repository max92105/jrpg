using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Content;

namespace Jrpg.Data.Tests.Battle;

// A rule's target against its skill's aim and its trigger (ADR 0057, decision 840): the message names the enemy, the
// rule's place, the skill and its aim.
public sealed class EnemyTacticRuleValidatorTests
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
        new[] { Skill("cleave", Targeting.AllEnemies), Skill("mend", Targeting.SingleAlly) }.ToDictionary(skill => skill.Id));

    [Fact]
    public void Load_ActionWithOn_ReadsTheTarget()
    {
        using var content = new CopiedContentRoot("enemy-rule-on", "test-world");
        content.Replace(
            "enemies.json",
            "\"id\": \"goblin\",",
            "\"id\": \"goblin\", \"tactics\": [ { \"weight\": 1, \"on\": \"lowestHpFoe\" } ],");

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal(TacticTarget.LowestHpFoe, catalog.Enemies["goblin"].Tactics[0].On);
    }

    [Fact]
    public void Validate_OnThatDoesntFitTheSkill_NamesTheEnemyTheRuleAndTheAim()
    {
        var cleave = new EnemyAction { SkillId = "cleave", Weight = 1, On = TacticTarget.LowestHpFoe };

        var problem = ProblemOf(new EnemyAction { Weight = 1 }, cleave);

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[1] (cleave).on: lowestHpFoe doesn't fit cleave, which aims at allEnemies.",
            problem);
    }

    // The trigger names fallen allies, and the attack aims at one foe.
    [Fact]
    public void Validate_SubjectThatDoesntFitTheAttack_NamesTheAttacksAim()
    {
        var attack = new EnemyAction
        {
            Weight = 1,
            When = new TacticCondition(TacticConditionKind.AllyFallen),
            On = TacticTarget.Subject,
        };

        var problem = ProblemOf(attack);

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0] (attack).on: subject doesn't fit attack, which aims at singleEnemy.",
            problem);
    }

    [Fact]
    public void Validate_OnThatFitsTheSkill_IsAccepted()
    {
        var mend = new EnemyAction
        {
            SkillId = "mend",
            Weight = 1,
            When = new TacticCondition(TacticConditionKind.AllyHpBelow, 50),
            On = TacticTarget.Subject,
        };

        Assert.Null(ProblemOf(mend));
    }

    [Fact]
    public void Validate_SubjectWithoutWhen_IsRefused()
    {
        var problem = ProblemOf(new EnemyAction { Weight = 1, On = TacticTarget.Subject });

        Assert.Equal("enemies.json: enemy 'goblin': tactics[0] (attack).on: subject needs a when that names someone.", problem);
    }

    [Fact]
    public void Validate_SubjectInAPhase_NamesThePhaseField()
    {
        var phase = new EnemyPhase
        {
            BelowHpPercent = 50,
            Tactics = [new EnemyAction { Weight = 1 }, new EnemyAction { Weight = 1, On = TacticTarget.Subject }],
        };

        var problem = ProblemOf(_goblin with { Phases = [phase] });

        Assert.Equal(
            "enemies.json: enemy 'goblin': phases[0].tactics[1] (attack).on: subject needs a when that names someone.",
            problem);
    }

    [Fact]
    public void Load_UnknownOn_NamesTheField()
    {
        using var content = new CopiedContentRoot("enemy-rule-unknown-on", "test-world");
        content.Replace(
            "enemies.json",
            "\"id\": \"goblin\",",
            "\"id\": \"goblin\", \"tactics\": [ { \"weight\": 1, \"on\": \"everyone\" } ],");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(content.Path));

        Assert.StartsWith("enemies.json at $[", exception.Message, StringComparison.Ordinal);
        Assert.Contains("].tactics[0].on", exception.Message, StringComparison.Ordinal);
    }

    private static string? ProblemOf(params EnemyAction[] tactics) => ProblemOf(_goblin with { Tactics = tactics });

    private static string? ProblemOf(EnemyDefinition enemy) =>
        Record.Exception(() => EnemyCatalogValidator.Validate("enemies.json", [enemy], _references))?.Message;

    private static SkillDefinition Skill(string id, Targeting targeting) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 5, targeting),
    };
}
