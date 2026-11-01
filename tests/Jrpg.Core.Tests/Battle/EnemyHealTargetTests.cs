using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A heal on one ally goes to the lowest HP share among those its condition found (decision 401). The heal is the only
// action, so nothing is drawn: an extra draw would run the empty sequence out.
public sealed class EnemyHealTargetTests
{
    private static readonly SkillDefinition _dressing = new()
    {
        Id = "dressing",
        Name = "Dressing",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.RestoreHp, 25, Targeting.SingleAlly),
    };

    private static readonly TacticCondition _allyBelowHalf = new(TacticConditionKind.AllyHpBelow, 50);

    [Fact]
    public void ChooseCommand_HealOnAllyBelowHalf_AimsAtTheLowestShareThatMetIt()
    {
        var lowest = Enemy("Lowest", hp: 45, maxHp: 100);
        var other = Enemy("Other", hp: 14, maxHp: 30);

        Assert.Equal(lowest, HealTargetOf(Actor(_allyBelowHalf), lowest, other));
    }

    [Fact]
    public void ChooseCommand_HealOnAllyBelowHalfLowestLast_AimsAtTheLowestShare()
    {
        var other = Enemy("Other", hp: 14, maxHp: 30);
        var lowest = Enemy("Lowest", hp: 45, maxHp: 100);

        Assert.Equal(lowest, HealTargetOf(Actor(_allyBelowHalf), other, lowest));
    }

    // The actor is lowest at 25 % and the first ally is above the threshold: only the second met the condition.
    [Fact]
    public void ChooseCommand_HealOnAllyBelowHalf_SkipsAWoundedAllyAboveTheThreshold()
    {
        var aboveIt = Enemy("Above", hp: 60, maxHp: 100);
        var metIt = Enemy("Met", hp: 49, maxHp: 100);

        Assert.Equal(metIt, HealTargetOf(Actor(_allyBelowHalf, hp: 60, maxHp: 240), aboveIt, metIt));
    }

    [Fact]
    public void ChooseCommand_HealWithTiedShares_AimsAtTheFirstInBattleOrder()
    {
        var first = Enemy("First", hp: 12, maxHp: 30);
        var second = Enemy("Second", hp: 40, maxHp: 100);

        Assert.Equal(first, HealTargetOf(Actor(_allyBelowHalf), first, second));
    }

    // The ally is lower, but "own HP below" found only the actor.
    [Fact]
    public void ChooseCommand_HealOnOwnHpBelow_AimsAtItself()
    {
        var actor = Actor(new TacticCondition(TacticConditionKind.OwnHpBelow, 50), hp: 40, maxHp: 100);

        Assert.Equal(actor, HealTargetOf(actor, Enemy("Ally", hp: 10, maxHp: 100), Enemy("Other", hp: 100, maxHp: 100)));
    }

    // Without an HP condition every legal target counts, the actor included, here the lowest.
    [Fact]
    public void ChooseCommand_HealWithoutACondition_AimsAtTheLowestShareOfAll()
    {
        var actor = Actor(condition: null, hp: 10, maxHp: 100);

        Assert.Equal(actor, HealTargetOf(actor, Enemy("Ally", hp: 20, maxHp: 100), Enemy("Other", hp: 30, maxHp: 100)));
    }

    // "An ally fallen" names nobody a heal can reach, so it too leaves every legal target.
    [Fact]
    public void ChooseCommand_HealOnAllyFallen_AimsAtTheLowestLivingShare()
    {
        var wounded = Enemy("Wounded", hp: 20, maxHp: 100);

        Assert.Equal(wounded, HealTargetOf(Actor(new TacticCondition(TacticConditionKind.AllyFallen)), wounded, Enemy("Fallen", hp: 0, maxHp: 100)));
    }

    // The sequence holds only the pick's draw, in the heal's share.
    [Fact]
    public void ChooseCommand_HealOnOneAlly_DrawsNothingForTheTarget()
    {
        var wounded = Enemy("Wounded", hp: 20, maxHp: 100);
        var actor = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, damage: 5).WithTactics(
            new EnemyAction { Weight = 1 },
            new EnemyAction { SkillId = _dressing.Id, Weight = 3, When = _allyBelowHalf }).Build());

        Assert.Equal(wounded, HealTargetOf(actor, wounded, Enemy("Other", hp: 100, maxHp: 100), draws: [1]));
    }

    // Shuffle values (0, 2, 1) swap the last combatant to the front, so the actor is first.
    private static Combatant HealTargetOf(Combatant actor, Combatant firstAlly, Combatant secondAlly, int[]? draws = null)
    {
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        var battle = new BattleState([member, firstAlly, secondAlly, actor], new Inventory(), new FixedSequenceRandom(0, 2, 1));
        var skills = new Dictionary<string, SkillDefinition> { [_dressing.Id] = _dressing };
        var command = new EnemyTactics(new FixedSequenceRandom(draws ?? []), skills).ChooseCommand(battle);
        return Assert.Single(Assert.IsType<SkillCommand>(command).Targets);
    }

    private static Combatant Actor(TacticCondition? condition, int hp = 100, int maxHp = 100)
    {
        var actor = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("Actor").WithStats(maxHp, damage: 5)
            .WithTactics(new EnemyAction { SkillId = _dressing.Id, Weight = 1, When = condition }).Build());
        actor.Vitals.TakeDamage(maxHp - hp);
        return actor;
    }

    private static Combatant Enemy(string name, int hp, int maxHp)
    {
        var enemy = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp, damage: 5).Build());
        enemy.Vitals.TakeDamage(maxHp - hp);
        return enemy;
    }
}
