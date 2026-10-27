using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A rogue with 3 readiness and a Focus that gains 1 at the end of each turn, against one goblin; the shuffle (1) keeps
// the rogue first. Dash is quick: it spends 1 readiness and deals 10 - 2 = 8 damage.
public sealed class BattleQuickActionTests
{
    private static readonly ResourceDefinition _readiness = new("readiness", "Readiness", Max: 3, StartsAt: 3);
    private static readonly ResourceDefinition _focus = new("focus", "Focus", Max: 5, StartsAt: 0) { Gains = new ResourceGains { PerTurn = 1 } };

    private static readonly SkillDefinition _dash = new()
    {
        Id = "dash",
        Name = "Dash",
        Description = "",
        IconFile = "",
        IsQuick = true,
        ResourceChanges = [new ResourceChange("readiness", -1)],
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };

    private readonly Combatant _rogue = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().WithId("lazarus").BuildDefinition(),
        new ClassBuilder().WithId("rogue").WithResources(_readiness, _focus).WithSkills("dash").Build(),
        new Dictionary<string, SkillDefinition> { ["dash"] = _dash },
        TestStatRules.Neutral));

    [Fact]
    public void Execute_QuickSkill_SameActorActsAgain()
    {
        var (battle, goblin) = Battle(goblinHp: 25);

        battle.Execute(new SkillCommand(_dash, [goblin]));

        Assert.Same(_rogue, battle.CurrentActor);
        Assert.Equal(BattlePhase.PartyTurn, battle.Phase);
    }

    [Fact]
    public void Execute_QuickSkill_NoUpkeepRuns()
    {
        var (battle, goblin) = Battle(goblinHp: 25);
        _rogue.Statuses.Apply(new StatusGrant(StatusKind.Guarded, Power: 5, Turns: 2));
        _rogue.Statuses.ClearFreshMarks();

        var events = battle.Execute(new SkillCommand(_dash, [goblin]));

        Assert.Equal(0, _rogue.Resources.AmountOf("focus"));
        Assert.DoesNotContain(new ResourceChanged(_rogue, "focus", 1), events);
        Assert.Equal(2, _rogue.Statuses.Active[0].TurnsLeft);
    }

    [Fact]
    public void Execute_AttackAfterQuickSkill_TurnEnds()
    {
        var (battle, goblin) = Battle(goblinHp: 25);
        battle.Execute(new SkillCommand(_dash, [goblin]));

        battle.Execute(new AttackCommand(goblin));

        Assert.Same(goblin, battle.CurrentActor);
        Assert.Equal(1, _rogue.Resources.AmountOf("focus"));
    }

    [Fact]
    public void Execute_QuickSkillFellsLastEnemy_BattleEnds()
    {
        var (battle, goblin) = Battle(goblinHp: 8);

        var events = battle.Execute(new SkillCommand(_dash, [goblin]));

        Assert.Equal(BattlePhase.Victory, battle.Phase);
        Assert.Equal(new BattleEnded(BattlePhase.Victory), events[^1]);
    }

    private (BattleState Battle, Combatant Goblin) Battle(int goblinHp)
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: goblinHp, attack: 20, defense: 2).Build());
        return (new BattleState([_rogue, goblin], new Inventory(), new FixedSequenceRandom(1)), goblin);
    }
}
