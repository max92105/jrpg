using Jrpg.Core.Battle;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Content;

namespace Jrpg.Data.Tests.Battle;

// A trigger's kind and its number, read from Core's shape table (ADR 0057): a reaction reads no number, every Nth turn
// reads its turns and the HP kinds their percent, and a number the kind doesn't read is left out.
public sealed class EnemyTacticConditionValidatorTests
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

    private static readonly EnemyReferences _references = new(new HashSet<string>(), new Dictionary<string, SkillDefinition>());

    [Fact]
    public void Validate_WasCriticallyHitWithAPercent_IsRefused()
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.WasCriticallyHit, 30) };

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0].when.percent must be left out of wasCriticallyHit (was 30).",
            ProblemOf(action));
    }

    [Fact]
    public void Validate_WasCriticallyHitWithoutAPercent_IsAccepted()
    {
        Assert.Null(ProblemOf(new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.WasCriticallyHit) }));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Validate_FoeHpBelowPercentAtABound_IsAccepted(int percent)
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.FoeHpBelow, percent) };

        Assert.Null(ProblemOf(action));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(11)]
    public void Validate_EveryNthTurnTurnsOutOfRange_IsRefused(int turns)
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.EveryNthTurn, Turns: turns) };

        Assert.Equal(
            $"enemies.json: enemy 'goblin': tactics[0].when.turns must be within 2..10 (was {turns}).",
            ProblemOf(action));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    public void Validate_EveryNthTurnTurnsAtABound_IsAccepted(int turns)
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.EveryNthTurn, Turns: turns) };

        Assert.Null(ProblemOf(action));
    }

    [Fact]
    public void Validate_TurnsOnAnHpKind_IsRefused()
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.AllyHpBelow, 50, Turns: 3) };

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0].when.turns must be left out of allyHpBelow (was 3).",
            ProblemOf(action));
    }

    [Fact]
    public void Validate_TurnsOnAReaction_IsRefused()
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.WasHit, Turns: 3) };

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0].when.turns must be left out of wasHit (was 3).",
            ProblemOf(action));
    }

    [Fact]
    public void Validate_PercentOnEveryNthTurn_IsRefused()
    {
        var action = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.EveryNthTurn, 50, 3) };

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0].when.percent must be left out of everyNthTurn (was 50).",
            ProblemOf(action));
    }

    [Fact]
    public void Load_WasCriticallyHitOnLowestHpFoe_ReadsTheRule()
    {
        using var content = new CopiedContentRoot("enemy-condition-reaction", "test-world");
        content.Replace(
            "enemies.json",
            "\"id\": \"gnoll\",",
            """
            "id": "gnoll", "tactics": [ { "weight": 1, "when": { "kind": "wasCriticallyHit" }, "on": "lowestHpFoe" } ],
            """);

        var expected = new EnemyAction
        {
            Weight = 1,
            When = new TacticCondition(TacticConditionKind.WasCriticallyHit),
            On = TacticTarget.LowestHpFoe,
        };

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal(expected, catalog.Enemies["gnoll"].Tactics[0]);
    }

    private static string? ProblemOf(EnemyAction action) =>
        Record.Exception(() => EnemyCatalogValidator.Validate("enemies.json", [_goblin with { Tactics = [action] }], _references))
            ?.Message;
}
