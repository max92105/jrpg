using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Slowed (-15 accuracy points, decision 807) on a member with 100 % accuracy, against a goblin with 10 % dodge: the hit
// chance drops from 90 % to 75 %. The shuffle (1) keeps the member first.
public sealed class BattleSlowedTests
{
    // Below 900, so it lands at 90 %; not below 750, so it misses at 75 %.
    private const int DrawBetweenTheChances = 800;

    [Fact]
    public void Execute_SlowedAttacker_MissesWhereItWouldHaveHit()
    {
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        member.Statuses.Apply(TestStatuses.Slowed(StatKind.Accuracy, -15, 3));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithDodge(10).Build());
        var battle = new BattleState([member, goblin], new Inventory(), new FixedSequenceRandom(1, DrawBetweenTheChances));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Contains(new HitMissed(goblin), events);
    }

    // The same draw against the unslowed chance, so the test above fails if the 90 % it starts from moves.
    [Fact]
    public void Execute_UnslowedAttacker_HitsAtTheSameDraw()
    {
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithDodge(10).Build());
        var battle = new BattleState([member, goblin], new Inventory(), new FixedSequenceRandom(1, DrawBetweenTheChances));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.DoesNotContain(new HitMissed(goblin), events);
    }
}
