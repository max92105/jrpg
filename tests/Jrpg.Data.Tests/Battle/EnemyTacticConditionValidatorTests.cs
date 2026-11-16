using Jrpg.Core.Battle;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Content;

namespace Jrpg.Data.Tests.Battle;

// A trigger's kind and its number, read from Core's shape table (ADR 0057): a reaction reads no number.
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

    [Fact]
    public void Load_WasCriticallyHitOnLowestHpFoe_ReadsTheRule()
    {
        using var content = new CopiedContentRoot("enemy-condition-reaction", "test-world");
        content.Replace(
            "enemies.json",
            "\"id\": \"goblin\",",
            """
            "id": "goblin", "tactics": [ { "weight": 1, "when": { "kind": "wasCriticallyHit" }, "on": "lowestHpFoe" } ],
            """);

        var expected = new EnemyAction
        {
            Weight = 1,
            When = new TacticCondition(TacticConditionKind.WasCriticallyHit),
            On = TacticTarget.LowestHpFoe,
        };

        var catalog = ContentCatalogLoader.Load(content.Path);

        Assert.Equal(expected, catalog.Enemies["goblin"].Tactics[0]);
    }

    private static string? ProblemOf(EnemyAction action) =>
        Record.Exception(() => EnemyCatalogValidator.Validate("enemies.json", [_goblin with { Tactics = [action] }], _references))
            ?.Message;
}
