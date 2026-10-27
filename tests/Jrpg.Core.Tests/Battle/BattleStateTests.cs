using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// The scripted shuffles: with two combatants, 1 keeps the listed order and 0 swaps it; with three, (2, 1) keeps it.
public sealed class BattleStateTests
{
    private readonly Combatant _hero = Hero(hp: 100);
    private readonly Combatant _goblin = Goblin(hp: 40);

    [Fact]
    public void Constructor_NoLivingEnemy_Throws()
    {
        _goblin.Vitals.TakeDamage(999);

        Assert.Throws<ArgumentException>(() => new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1)));
    }

    // Decision 97: a battle that starts with the whole party down is a defeat, so the defeat screen offers its choices.
    [Fact]
    public void Constructor_NobodyStanding_IsDefeatAtOnce()
    {
        _hero.Vitals.TakeDamage(999);

        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));

        Assert.Equal(BattlePhase.Defeat, battle.Phase);
        Assert.Throws<InvalidOperationException>(() => battle.Execute(new DefendCommand()));
    }

    [Fact]
    public void Constructor_SameCombatantTwice_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BattleState([_hero, _goblin, _goblin], new Inventory(), new FixedSequenceRandom(2, 1)));
    }

    [Fact]
    public void Execute_Attack_ReducesTargetHpByFormula()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));

        battle.Execute(new AttackCommand(_goblin));

        // Hero attack 15 - goblin defense 2.
        Assert.Equal(27, _goblin.Vitals.Hp);
    }

    [Fact]
    public void Execute_Attack_EndsTurnAndReportsActorFirst()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));
        var attack = new AttackCommand(_goblin);

        var events = battle.Execute(attack);

        Assert.Equal([new ActionStarted(_hero, attack), new EffectApplied(_goblin, EffectKind.Damage, 13)], events);
        Assert.Same(_goblin, battle.CurrentActor);
        Assert.Equal(BattlePhase.EnemyTurn, battle.Phase);
    }

    [Fact]
    public void Execute_AttackOnDefeatedTarget_ThrowsAndChangesNothing()
    {
        var fallenGoblin = Goblin(hp: 40);
        fallenGoblin.Vitals.TakeDamage(999);
        var battle = new BattleState([_hero, _goblin, fallenGoblin], new Inventory(), new FixedSequenceRandom(2, 1));

        Assert.Throws<ArgumentException>(() => battle.Execute(new AttackCommand(fallenGoblin)));

        AssertStillHerosTurn(battle);
    }

    [Fact]
    public void Execute_AttackOnOwnSide_ThrowsAndChangesNothing()
    {
        var ally = Hero(hp: 100);
        var battle = new BattleState([_hero, ally, _goblin], new Inventory(), new FixedSequenceRandom(2, 1));

        Assert.Throws<ArgumentException>(() => battle.Execute(new AttackCommand(ally)));

        AssertStillHerosTurn(battle);
        Assert.Equal(100, ally.Vitals.Hp);
    }

    [Fact]
    public void Execute_AttackOnCombatantOutsideBattle_ThrowsAndChangesNothing()
    {
        var outsider = Goblin(hp: 40);
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));

        Assert.Throws<ArgumentException>(() => battle.Execute(new AttackCommand(outsider)));

        AssertStillHerosTurn(battle);
        Assert.Equal(40, outsider.Vitals.Hp);
    }

    [Fact]
    public void Execute_AfterBattleEnded_Throws()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new FleeCommand());

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new DefendCommand()));
    }

    [Fact]
    public void Execute_Defend_ReturnsOnlyActionStartedAndEndsTurn()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));
        var defend = new DefendCommand();

        var events = battle.Execute(defend);

        Assert.Equal([new ActionStarted(_hero, defend)], events);
        Assert.Same(_goblin, battle.CurrentActor);
    }

    [Fact]
    public void Execute_Flee_PhaseIsEscaped()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));
        var flee = new FleeCommand();

        var events = battle.Execute(flee);

        Assert.Equal([new ActionStarted(_hero, flee), new BattleEnded(BattlePhase.Escaped)], events);
        Assert.Equal(BattlePhase.Escaped, battle.Phase);
    }

    [Fact]
    public void Execute_FleeOnEnemyTurn_ThrowsAndChangesNothing()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(0));

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new FleeCommand()));

        Assert.Same(_goblin, battle.CurrentActor);
        Assert.Equal(BattlePhase.EnemyTurn, battle.Phase);
    }

    [Fact]
    public void Start_PartyAndEnemies_CreatesCombatantsInPartyThenEncounterOrder()
    {
        var sarxian = new PartyMemberBuilder().WithId("sarxian").Build();
        var jazog = new PartyMemberBuilder().WithId("jazog").Build();
        var reserve = new PartyMemberBuilder().WithId("reserve").Build();
        var party = new PartyState([sarxian, jazog, reserve], [sarxian, jazog], new PartyRules(MaxActiveMembers: 4));
        EnemyDefinition[] enemies = [Enemy("Goblin A"), Enemy("Goblin B"), Enemy("Goblin C")];

        var battle = BattleState.Start(party, enemies, new Inventory(), new FixedSequenceRandom(4, 3, 2, 1));

        Assert.Equal(
            [(BattleSide.Party, "Test Member"), (BattleSide.Party, "Test Member"), (BattleSide.Enemies, "Goblin A"), (BattleSide.Enemies, "Goblin B"), (BattleSide.Enemies, "Goblin C")],
            battle.Combatants.Select(combatant => (combatant.Side, combatant.Name)));
        Assert.Equal([sarxian, jazog], battle.Combatants.Take(2).Select(combatant => combatant.Member));
    }

    [Fact]
    public void UpcomingActors_FourCombatants_ListsCycleAfterCurrent()
    {
        var (battle, goblins) = HeroAgainstThreeWeakGoblins();

        var upcoming = battle.UpcomingActors(3);

        Assert.Equal(goblins, upcoming);
    }

    [Fact]
    public void UpcomingActors_DefeatedCombatant_IsSkipped()
    {
        var (battle, goblins) = HeroAgainstThreeWeakGoblins();
        battle.Execute(new AttackCommand(goblins[0]));

        var upcoming = battle.UpcomingActors(3);

        Assert.Equal([goblins[2], _hero, goblins[1]], upcoming);
    }

    [Fact]
    public void UpcomingActors_CountPastCycle_Wraps()
    {
        var (battle, goblins) = HeroAgainstThreeWeakGoblins();

        var upcoming = battle.UpcomingActors(5);

        Assert.Equal([.. goblins, _hero, goblins[0]], upcoming);
    }

    private static EnemyDefinition Enemy(string name, int maxHp = 40) =>
        new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp, attack: 20, defense: 2).Build();

    // The shuffle (3, 2, 1) keeps the listed order: the hero, then the goblins. One hit fells a 10 HP goblin.
    private (BattleState Battle, Combatant[] Goblins) HeroAgainstThreeWeakGoblins()
    {
        Combatant[] goblins = [Goblin(hp: 10), Goblin(hp: 10), Goblin(hp: 10)];
        return (new BattleState([_hero, .. goblins], new Inventory(), new FixedSequenceRandom(3, 2, 1)), goblins);
    }

    private static Combatant Hero(int hp) => Combatant.FromPartyMember(
        new PartyMemberBuilder().WithStats(attack: 15, defense: 8).WithVitals(hp, maxHp: 100).Build());

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, attack: 20, defense: 2).Build());

    private void AssertStillHerosTurn(BattleState battle)
    {
        Assert.Same(_hero, battle.CurrentActor);
        Assert.Equal(BattlePhase.PartyTurn, battle.Phase);
    }
}
