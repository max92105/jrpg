using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Content;

namespace Jrpg.Data.Tests.Battle;

// A rule's target against its skill's aim and its trigger (ADR 0057, decision 840), and a foe without its statuses
// against its grants: the message names the enemy, the rule's place, the skill and its aim. A rule's flags: the attack
// can't be once per battle (decision 842).
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
        new[]
        {
            Skill("cleave", Targeting.AllEnemies),
            Skill("mend", Targeting.SingleAlly),
            Skill("jab", Targeting.SingleEnemy),
            PoisonSkill("poison_dart"),
        }.ToDictionary(skill => skill.Id));

    [Fact]
    public void Load_ActionWithOn_ReadsTheTarget()
    {
        using var content = new CopiedContentRoot("enemy-rule-on", "test-world");
        content.Replace(
            "enemies.json",
            "\"id\": \"gnoll\",",
            "\"id\": \"gnoll\", \"tactics\": [ { \"weight\": 1, \"on\": \"lowestHpFoe\" } ],");

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal(TacticTarget.LowestHpFoe, catalog.Enemies["gnoll"].Tactics[0].On);
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

    // The jab aims at one foe as the target does, so what it lacks is a harmful grant.
    [Fact]
    public void Validate_UnafflictedFoeOnAPlainHit_IsRefused()
    {
        var problem = ProblemOf(new EnemyAction { SkillId = "jab", Weight = 1, On = TacticTarget.UnafflictedFoe });

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0] (jab).on: unafflictedFoe needs a skill that grants a harmful status.",
            problem);
    }

    [Fact]
    public void Validate_UnafflictedFoeOnAHeal_NamesTheAim()
    {
        var problem = ProblemOf(new EnemyAction { SkillId = "mend", Weight = 1, On = TacticTarget.UnafflictedFoe });

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0] (mend).on: unafflictedFoe doesn't fit mend, which aims at singleAlly.",
            problem);
    }

    [Fact]
    public void Validate_UnafflictedFoeOnAPoisonSkill_IsAccepted()
    {
        Assert.Null(ProblemOf(new EnemyAction { SkillId = "poison_dart", Weight = 1, On = TacticTarget.UnafflictedFoe }));
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
    public void Validate_OnceOnTheAttack_IsRefused()
    {
        var problem = ProblemOf(new EnemyAction { Weight = 1, IsOncePerBattle = true });

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0] (attack).isOncePerBattle: the attack can't be once per battle.",
            problem);
    }

    [Fact]
    public void Validate_OnceOnASkill_IsAccepted()
    {
        Assert.Null(ProblemOf(new EnemyAction { SkillId = "cleave", Weight = 1, IsOncePerBattle = true }));
    }

    [Fact]
    public void Load_UnknownOn_NamesTheField()
    {
        using var content = new CopiedContentRoot("enemy-rule-unknown-on", "test-world");
        content.Replace(
            "enemies.json",
            "\"id\": \"gnoll\",",
            "\"id\": \"gnoll\", \"tactics\": [ { \"weight\": 1, \"on\": \"everyone\" } ],");

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

    // A single-foe hit granting a harmful status, as content's poison is.
    private static SkillDefinition PoisonSkill(string id) => Skill(id, Targeting.SingleEnemy) with
    {
        Effect = new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy) { Statuses = [new StatusGrant(Poison, 3)] },
    };

    private static StatusDefinition Poison => new()
    {
        Id = "poison",
        Name = "Poison",
        Description = "",
        IsHarmful = true,
        Parts = [new StatusPart(StatusPartKind.DamageEachTurn, 2)],
    };
}
