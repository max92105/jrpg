using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Who the actor can aim an attack at (TargetChoices). The scripted shuffles: with two combatants, 1 keeps the listed
// order; with three, (2, 1) keeps it.
public sealed class BattleStateTargetTests
{
    private readonly Combatant _hero = Hero(hp: 100);
    private readonly Combatant _goblin = Goblin(hp: 40);

    [Fact]
    public void TargetChoiceForAttack_DefeatedEnemy_IsExcluded()
    {
        var fallen = Goblin(hp: 40);
        fallen.Vitals.TakeDamage(999);
        var battle = new BattleState([_hero, fallen, _goblin], new Inventory(), new FixedSequenceRandom(2, 1));

        var choice = battle.TargetChoiceForAttack();

        Assert.Equal([_goblin], choice.Candidates);
    }

    [Fact]
    public void TargetChoiceForAttack_TwoLivingEnemies_InFormationOrder()
    {
        var second = Goblin(hp: 40);
        var battle = new BattleState([_hero, _goblin, second], new Inventory(), new FixedSequenceRandom(2, 1));

        var choice = battle.TargetChoiceForAttack();

        Assert.Equal([_goblin, second], choice.Candidates);
    }

    [Fact]
    public void TargetChoiceForAttack_IsSingleTarget()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));

        var choice = battle.TargetChoiceForAttack();

        Assert.False(choice.TakesAll);
    }

    private static Combatant Hero(int hp) => Combatant.FromPartyMember(
        new PartyMemberBuilder().WithAttack(13).WithVitals(hp, maxHp: 100).Build());

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, attack: 12).Build());
}
