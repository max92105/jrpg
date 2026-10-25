using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

public sealed class TurnOrderTests
{
    private readonly Combatant _first = Enemy("First");
    private readonly Combatant _second = Enemy("Second");
    private readonly Combatant _third = Enemy("Third");

    [Fact]
    public void Constructor_FixedRandomValues_ShufflesWithFisherYates()
    {
        // Swap index 2 with 0, then index 1 with 0: [First, Second, Third] -> [Third, Second, First] -> [Second, Third, First].
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(0, 0));

        Assert.Equal([_second, _third, _first], TakeTurns(order, 3));
    }

    [Fact]
    public void Constructor_Seed7_ProducesKnownOrder()
    {
        var fourth = Enemy("Fourth");
        var fifth = Enemy("Fifth");

        var order = new TurnOrder([_first, _second, _third, fourth, fifth], new SeededRandom(7));

        Assert.Equal([_third, _first, fifth, fourth, _second], TakeTurns(order, 5));
    }

    [Fact]
    public void Constructor_FirstInShuffledOrderDefeated_StartsWithNextLiving()
    {
        // The scripted shuffle gives [Second, Third, First].
        _second.Vitals.TakeDamage(999);

        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(0, 0));

        Assert.Same(_third, order.Current);
    }

    [Fact]
    public void Constructor_NoOneAlive_Throws()
    {
        _first.Vitals.TakeDamage(999);

        Assert.Throws<ArgumentException>(() => new TurnOrder([_first], new FixedSequenceRandom()));
    }

    [Fact]
    public void Advance_PastLastCombatant_WrapsToFirst()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(0, 0));

        var turns = TakeTurns(order, 4);

        Assert.Equal([_second, _third, _first, _second], turns);
    }

    [Fact]
    public void Advance_NextCombatantDefeated_SkipsIt()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(0, 0));
        _third.Vitals.TakeDamage(999);

        order.Advance();

        Assert.Same(_first, order.Current);
    }

    [Fact]
    public void Advance_OnlyCurrentAlive_StaysOnCurrent()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(0, 0));
        _third.Vitals.TakeDamage(999);
        _first.Vitals.TakeDamage(999);

        order.Advance();

        Assert.Same(_second, order.Current);
    }

    [Fact]
    public void Advance_EveryoneDefeated_Throws()
    {
        var order = new TurnOrder([_first], new FixedSequenceRandom());
        _first.Vitals.TakeDamage(999);

        Assert.Throws<InvalidOperationException>(order.Advance);
    }

    [Fact]
    public void Upcoming_EveryoneDefeated_Throws()
    {
        var order = new TurnOrder([_first], new FixedSequenceRandom());
        _first.Vitals.TakeDamage(999);

        Assert.Throws<InvalidOperationException>(() => order.Upcoming(3));
    }

    // The shuffle (2, 1) keeps [First, Second, Third]. First readies an extra turn after Second.
    [Fact]
    public void Advance_ReadiedAfterB_ActorActsRightAfterB()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(2, 1));
        order.ReadyExtraTurn(_first, _second);

        var turns = TakeTurns(order, 5);

        Assert.Equal([_first, _second, _first, _third, _first], turns);
    }

    // First and Third both ready after Second: both extra turns follow Second's next turn, one after the other.
    [Fact]
    public void Advance_TwoReadiedOnOneAnchor_BothRightAfterIt()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(2, 1));
        order.ReadyExtraTurn(_first, _second);
        order.ReadyExtraTurn(_third, _second);
        var upcoming = order.Upcoming(4);

        var turns = TakeTurns(order, 5);

        Assert.Equal([_second, _first, _third, _third], upcoming);
        Assert.Equal([_first, _second, _first, _third, _third], turns);
    }

    [Fact]
    public void Upcoming_ReadiedTurn_ListedAfterAnchor()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(2, 1));
        order.ReadyExtraTurn(_first, _second);

        var upcoming = order.Upcoming(4);

        Assert.Equal([_second, _first, _third, _first], upcoming);
    }

    [Fact]
    public void Advance_AnchorFellFirst_ExtraTurnAfterNextTurn()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(2, 1));
        order.ReadyExtraTurn(_first, _third);
        _third.Vitals.TakeDamage(999);

        var turns = TakeTurns(order, 3);

        Assert.Equal([_first, _second, _first], turns);
    }

    [Fact]
    public void Advance_ReadiedTurn_IsExtraTurn()
    {
        var order = new TurnOrder([_first, _second, _third], new FixedSequenceRandom(2, 1));
        order.ReadyExtraTurn(_first, _second);
        order.Advance();
        var onSecondsTurn = order.IsExtraTurn;

        order.Advance();

        Assert.False(onSecondsTurn);
        Assert.True(order.IsExtraTurn);
        Assert.False(order.HasReadiedTurn(_first));
    }

    private static Combatant Enemy(string name) => Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName(name).Build());

    private static List<Combatant> TakeTurns(TurnOrder order, int count)
    {
        var turns = new List<Combatant> { order.Current };
        for (var turn = 1; turn < count; turn++)
        {
            order.Advance();
            turns.Add(order.Current);
        }

        return turns;
    }
}
