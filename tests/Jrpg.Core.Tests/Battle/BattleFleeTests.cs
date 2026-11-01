using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// No running while a boss is in the battle, standing or fallen (decision 407). With three combatants the shuffle (2, 1)
// keeps the listed order; with two, 1 keeps it and 0 puts the enemy first.
public sealed class BattleFleeTests
{
    private readonly Combatant _member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
    private readonly Combatant _goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
    private readonly Combatant _boss = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("Chief").AsBoss().Build());

    [Fact]
    public void CanFlee_PartyTurnNoBoss_IsTrue()
    {
        var battle = new BattleState([_member, _goblin], new Inventory(), new FixedSequenceRandom(1));

        Assert.True(battle.CanFlee);
    }

    [Fact]
    public void CanFlee_BossInTheBattle_IsFalse()
    {
        var battle = new BattleState([_member, _boss, _goblin], new Inventory(), new FixedSequenceRandom(2, 1));

        Assert.Same(_member, battle.CurrentActor);
        Assert.False(battle.CanFlee);
    }

    [Fact]
    public void CanFlee_BossFallen_IsStillFalse()
    {
        var battle = new BattleState([_member, _boss, _goblin], new Inventory(), new FixedSequenceRandom(2, 1));

        _boss.Vitals.TakeDamage(_boss.Vitals.MaxHp);

        Assert.Same(_member, battle.CurrentActor);
        Assert.False(battle.CanFlee);
    }

    [Fact]
    public void CanFlee_EnemyTurn_IsFalse()
    {
        var battle = new BattleState([_member, _goblin], new Inventory(), new FixedSequenceRandom(0));

        Assert.False(battle.CanFlee);
    }

    [Fact]
    public void Execute_FleeFromABoss_Throws()
    {
        var battle = new BattleState([_member, _boss, _goblin], new Inventory(), new FixedSequenceRandom(2, 1));

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new FleeCommand()));

        Assert.Equal(BattlePhase.PartyTurn, battle.Phase);
    }
}
