using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A hero defends against goblins (attack 20); the shuffle keeps the encounter's order, so each goblin's turn starts
// within the hero's Execute. The statuses are applied straight onto their holder (decision 804).
public sealed class BattleTurnStartTickTests
{
    private const string BurningId = "burning";

    private static readonly SkillDefinition _ready = new()
    {
        Id = "ready",
        Name = "ready",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.ReadyExtraTurn, 0, Targeting.OtherCombatant),
    };

    private readonly Combatant _hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_ready).Build());

    [Fact]
    public void Execute_StartOfTurnStatus_TicksWhenTheHoldersTurnStarts()
    {
        var goblin = Goblin(hp: 25);
        goblin.Statuses.Apply(Burning(StatusTiming.StartOfTurn, amount: 5));
        var battle = Battle(goblin);

        var events = battle.Execute(new DefendCommand());

        var ticked = Assert.IsType<StatusTicked>(events[^1]);
        Assert.Equal((goblin, BurningId, 5), (ticked.Holder, TestStatuses.IdOf(ticked.Status), ticked.Amount));
        Assert.Same(goblin, battle.CurrentActor);
    }

    [Fact]
    public void Execute_StartOfTurnStatus_DoesNotTickAtTheEnd()
    {
        var goblin = Goblin(hp: 25);
        goblin.Statuses.Apply(Burning(StatusTiming.StartOfTurn, amount: 5));
        var battle = Battle(goblin);
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new DefendCommand());

        Assert.DoesNotContain(events, battleEvent => battleEvent is StatusTicked);
        Assert.Equal(20, goblin.Vitals.Hp);
    }

    [Fact]
    public void Execute_EndOfTurnStatus_StillTicksAtTheEnd()
    {
        var goblin = Goblin(hp: 25);
        goblin.Statuses.Apply(Burning(StatusTiming.EndOfTurn, amount: 5));
        var battle = Battle(goblin);
        var heroTurn = battle.Execute(new DefendCommand());

        var events = battle.Execute(new DefendCommand());

        Assert.DoesNotContain(heroTurn, battleEvent => battleEvent is StatusTicked);
        Assert.Contains(events, battleEvent => battleEvent is StatusTicked ticked && ticked.Holder == goblin);
    }

    [Fact]
    public void Execute_StartTickFellsTheHolder_TurnPassesToTheNext()
    {
        var burned = Goblin(hp: 5);
        burned.Statuses.Apply(Burning(StatusTiming.StartOfTurn, amount: 10));
        var next = Goblin(hp: 25);
        var battle = Battle(burned, next);

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(new CombatantDefeated(burned), events);
        Assert.Same(next, battle.CurrentActor);
        Assert.Equal(BattlePhase.EnemyTurn, battle.Phase);
    }

    [Fact]
    public void Execute_StartTickFellsTheLastEnemy_EndsInVictory()
    {
        var goblin = Goblin(hp: 5);
        goblin.Statuses.Apply(Burning(StatusTiming.StartOfTurn, amount: 10));
        var battle = Battle(goblin);

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(new CombatantDefeated(goblin), events);
        Assert.Equal(new BattleEnded(BattlePhase.Victory), events[^1]);
        Assert.Equal(BattlePhase.Victory, battle.Phase);
    }

    // The hero's upkeep clears the stun's fresh mark, so the skipped turn counts it down and the hero acts next.
    [Fact]
    public void Execute_StartTickOnAStunnedHolder_TicksThenSkips()
    {
        var goblin = Goblin(hp: 25);
        goblin.Statuses.Apply(Burning(StatusTiming.StartOfTurn, amount: 5));
        goblin.Statuses.Apply(TestStatuses.Stun(100, 1));
        var battle = Battle(goblin);

        var events = battle.Execute(new DefendCommand()).ToList();

        var tick = events.FindIndex(battleEvent => battleEvent is StatusTicked ticked && ticked.Holder == goblin);
        var skip = events.FindIndex(battleEvent => battleEvent is TurnSkipped skipped && skipped.Combatant == goblin);
        Assert.Equal(tick + 1, skip);
        Assert.Same(_hero, battle.CurrentActor);
    }

    // The Ready puts the hero's extra turn right after the goblin's (decision 202): no upkeep, no start tick.
    [Fact]
    public void Execute_ReadiedExtraTurn_HasNoStartTick()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);
        battle.Execute(new SkillCommand(_ready, [goblin]));
        _hero.Statuses.Apply(Burning(StatusTiming.StartOfTurn, amount: 5));

        var events = battle.Execute(new DefendCommand());

        Assert.Same(_hero, battle.CurrentActor);
        Assert.DoesNotContain(events, battleEvent => battleEvent is StatusTicked);
    }

    // Granted for one turn, it ticks as the goblin's turn starts and ends as it ends (ADR 0021).
    [Fact]
    public void Execute_StartOfTurnStatus_CountsDownAtTheEnd()
    {
        var goblin = Goblin(hp: 25);
        goblin.Statuses.Apply(Burning(StatusTiming.StartOfTurn, amount: 5, turns: 1));
        var battle = Battle(goblin);
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(
            events.OfType<StatusEnded>(),
            ended => (ended.Holder, TestStatuses.IdOf(ended.Status)) == (goblin, BurningId));
        Assert.Equal(20, goblin.Vitals.Hp);
    }

    // Fisher-Yates swaps each last index with the value drawn, so drawing the index itself keeps the order.
    private BattleState Battle(params Combatant[] enemies)
    {
        int[] keepOrder = [.. Enumerable.Range(1, enemies.Length).Reverse()];
        return new BattleState([_hero, .. enemies], new Inventory(), new FixedSequenceRandom(keepOrder));
    }

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, damage: 20).Build());

    // No other test grants a start-of-turn tick, so its record is built here.
    private static StatusGrant Burning(StatusTiming actsAt, int amount, int turns = 3) => new(
        new StatusDefinition
        {
            Id = BurningId,
            Name = BurningId,
            Description = "",
            ActsAt = actsAt,
            Parts = [new StatusPart(StatusPartKind.DamageEachTurn, amount)],
        },
        turns);
}
