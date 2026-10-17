using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// The scripted shuffles: with two combatants, 1 keeps the listed order and 0 swaps it; with three, (2, 1) keeps it.
public sealed class BattleStateTests
{
    private readonly Combatant _hero = Hero(hp: 100);
    private readonly Combatant _goblin = Goblin(hp: 40);

    [Theory]
    [InlineData(BattleSide.Party)]
    [InlineData(BattleSide.Enemies)]
    public void Constructor_SideWithNobodyAlive_Throws(BattleSide fallenSide)
    {
        var fallen = fallenSide == BattleSide.Party ? _hero : _goblin;
        fallen.Vitals.TakeDamage(999);

        Assert.Throws<ArgumentException>(() => new BattleState([_hero, _goblin], new FixedSequenceRandom(1)));
    }

    [Fact]
    public void Constructor_SameCombatantTwice_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BattleState([_hero, _goblin, _goblin], new FixedSequenceRandom(2, 1)));
    }

    [Fact]
    public void Execute_Attack_ReducesTargetHpByFormula()
    {
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(1));

        battle.Execute(new AttackCommand(_goblin));

        // Hero attack 15 - goblin defense 2.
        Assert.Equal(27, _goblin.Vitals.Hp);
    }

    [Fact]
    public void Execute_Attack_EndsTurnAndReportsActorFirst()
    {
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(1));
        var attack = new AttackCommand(_goblin);

        var events = battle.Execute(attack);

        Assert.Equal([new ActionStarted(_hero, attack), new EffectApplied(_goblin, EffectKind.Damage, 13)], events);
        Assert.Same(_goblin, battle.CurrentActor);
        Assert.Equal(BattlePhase.EnemyTurn, battle.Phase);
    }

    [Fact]
    public void Execute_LastEnemyDefeated_EndsWithDefeatedAndVictoryEvents()
    {
        var weakGoblin = Goblin(hp: 10);
        var battle = new BattleState([_hero, weakGoblin], new FixedSequenceRandom(1));
        var attack = new AttackCommand(weakGoblin);

        var events = battle.Execute(attack);

        BattleEvent[] expected =
        [
            new ActionStarted(_hero, attack),
            new EffectApplied(weakGoblin, EffectKind.Damage, 10),
            new CombatantDefeated(weakGoblin),
            new BattleEnded(BattlePhase.Victory),
        ];
        Assert.Equal(expected, events);
        Assert.Equal(BattlePhase.Victory, battle.Phase);
    }

    [Fact]
    public void Execute_LastPartyMemberDefeated_EndsWithDefeatedAndDefeatEvents()
    {
        var weakHero = Hero(hp: 5);
        var battle = new BattleState([weakHero, _goblin], new FixedSequenceRandom(0));
        var attack = new AttackCommand(weakHero);

        var events = battle.Execute(attack);

        BattleEvent[] expected =
        [
            new ActionStarted(_goblin, attack),
            new EffectApplied(weakHero, EffectKind.Damage, 5),
            new CombatantDefeated(weakHero),
            new BattleEnded(BattlePhase.Defeat),
        ];
        Assert.Equal(expected, events);
        Assert.Equal(BattlePhase.Defeat, battle.Phase);
    }

    [Fact]
    public void Execute_AttackOnDefeatedTarget_ThrowsAndChangesNothing()
    {
        var fallenGoblin = Goblin(hp: 40);
        fallenGoblin.Vitals.TakeDamage(999);
        var battle = new BattleState([_hero, _goblin, fallenGoblin], new FixedSequenceRandom(2, 1));

        Assert.Throws<ArgumentException>(() => battle.Execute(new AttackCommand(fallenGoblin)));

        AssertStillHerosTurn(battle);
    }

    [Fact]
    public void Execute_AttackOnOwnSide_ThrowsAndChangesNothing()
    {
        var ally = Hero(hp: 100);
        var battle = new BattleState([_hero, ally, _goblin], new FixedSequenceRandom(2, 1));

        Assert.Throws<ArgumentException>(() => battle.Execute(new AttackCommand(ally)));

        AssertStillHerosTurn(battle);
        Assert.Equal(100, ally.Vitals.Hp);
    }

    [Fact]
    public void Execute_AttackOnCombatantOutsideBattle_ThrowsAndChangesNothing()
    {
        var outsider = Goblin(hp: 40);
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(1));

        Assert.Throws<ArgumentException>(() => battle.Execute(new AttackCommand(outsider)));

        AssertStillHerosTurn(battle);
        Assert.Equal(40, outsider.Vitals.Hp);
    }

    [Fact]
    public void Execute_AfterBattleEnded_Throws()
    {
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(1));
        battle.Execute(new FleeCommand());

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new DefendCommand()));
    }

    [Fact]
    public void Execute_Defend_ReturnsOnlyActionStartedAndEndsTurn()
    {
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(1));
        var defend = new DefendCommand();

        var events = battle.Execute(defend);

        Assert.Equal([new ActionStarted(_hero, defend)], events);
        Assert.Same(_goblin, battle.CurrentActor);
    }

    [Fact]
    public void Execute_Flee_PhaseIsEscaped()
    {
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(1));
        var flee = new FleeCommand();

        var events = battle.Execute(flee);

        Assert.Equal([new ActionStarted(_hero, flee), new BattleEnded(BattlePhase.Escaped)], events);
        Assert.Equal(BattlePhase.Escaped, battle.Phase);
    }

    [Fact]
    public void Execute_FleeOnEnemyTurn_ThrowsAndChangesNothing()
    {
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(0));

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new FleeCommand()));

        Assert.Same(_goblin, battle.CurrentActor);
        Assert.Equal(BattlePhase.EnemyTurn, battle.Phase);
    }

    [Fact]
    public void Execute_AttackUntilOver_EndsInVictoryAfterThreeGoblinHits()
    {
        // The hero acts first and deals 13 per hit, felling the goblin (40 HP) on its 4th turn;
        // the goblin gets 3 turns in between, dealing 12 each.
        var battle = new BattleState([_hero, _goblin], new FixedSequenceRandom(1));
        var tactics = new EnemyTactics(new FixedSequenceRandom(0, 0, 0));

        for (var turn = 0; turn < 20 && !battle.IsOver; turn++)
        {
            var command = battle.Phase == BattlePhase.PartyTurn ? new AttackCommand(_goblin) : tactics.ChooseCommand(battle);
            battle.Execute(command);
        }

        Assert.Equal(BattlePhase.Victory, battle.Phase);
        Assert.Equal(100 - (3 * 12), _hero.Vitals.Hp);
    }

    private static Combatant Hero(int hp) => Combatant.FromPartyMember(
        new PartyMemberBuilder().WithStats(attack: 15, defense: 8).WithVitals(hp, maxHp: 100, mp: 0, maxMp: 0).Build());

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, attack: 20, defense: 2).Build());

    private void AssertStillHerosTurn(BattleState battle)
    {
        Assert.Same(_hero, battle.CurrentActor);
        Assert.Equal(BattlePhase.PartyTurn, battle.Phase);
    }
}
