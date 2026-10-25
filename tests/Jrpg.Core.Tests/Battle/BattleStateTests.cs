using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
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
    public void Execute_LastEnemyDefeated_EndsWithDefeatedAndVictoryEvents()
    {
        var weakGoblin = Goblin(hp: 10);
        var battle = new BattleState([_hero, weakGoblin], new Inventory(), new FixedSequenceRandom(1));
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
        var battle = new BattleState([weakHero, _goblin], new Inventory(), new FixedSequenceRandom(0));
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
    public void Execute_AttackUntilOver_EndsInVictoryAfterThreeGoblinHits()
    {
        // The hero acts first and deals 13 per hit, felling the goblin (40 HP) on its 4th turn;
        // the goblin gets 3 turns in between, dealing 12 each.
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));
        var tactics = new EnemyTactics(new FixedSequenceRandom(0, 0, 0));

        for (var turn = 0; turn < 20 && !battle.IsOver; turn++)
        {
            var command = battle.Phase == BattlePhase.PartyTurn ? new AttackCommand(_goblin) : tactics.ChooseCommand(battle);
            battle.Execute(command);
        }

        Assert.Equal(BattlePhase.Victory, battle.Phase);
        Assert.Equal(100 - (3 * 12), _hero.Vitals.Hp);
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
    public void GrantVictoryExperience_AfterVictory_GivesEnemyExperienceToLivingMembers()
    {
        var member = new PartyMemberBuilder().WithStats(attack: 15, defense: 8).Build();
        var party = new PartyState([member], [member], new PartyRules(MaxActiveMembers: 4));
        var battle = BattleState.Start(party, [Enemy("Goblin", maxHp: 10)], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new AttackCommand(battle.LivingOn(BattleSide.Enemies)[0]));

        var gains = battle.GrantVictoryExperience(new ExperienceTable([1000])).Gains;

        var gain = Assert.Single(gains);
        Assert.Same(member, gain.Member);
        Assert.Equal(40, gain.Amount);
        Assert.Equal(40, member.Experience);
    }

    [Fact]
    public void GrantVictoryExperience_ThreeGoblins_ExperienceIsSumOfRewards()
    {
        var sarxian = new PartyMemberBuilder().WithStats(attack: 15, defense: 8).Build();
        var jazog = new PartyMemberBuilder().WithStats(attack: 15, defense: 8).Build();
        var party = new PartyState([sarxian, jazog], [sarxian, jazog], new PartyRules(MaxActiveMembers: 4));
        EnemyDefinition[] goblins = [Enemy("Goblin A", maxHp: 10), Enemy("Goblin B", maxHp: 10), Enemy("Goblin C", maxHp: 10)];
        var battle = BattleState.Start(party, goblins, new Inventory(), new FixedSequenceRandom(4, 3, 2, 1));
        BattleScript.AttackUntilOver(battle);

        var rewards = battle.GrantVictoryExperience(new ExperienceTable([1000]));

        Assert.Equal(120, rewards.Experience);
        Assert.Equal([60, 60], rewards.Gains.Select(gain => gain.Amount));
    }

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

    [Fact]
    public void GrantVictoryExperience_BeforeTheBattleIsWon_Throws()
    {
        var battle = new BattleState([_hero, _goblin], new Inventory(), new FixedSequenceRandom(1));

        Assert.Throws<InvalidOperationException>(() => battle.GrantVictoryExperience(new ExperienceTable([1000])));
    }

    [Fact]
    public void GrantVictoryExperience_CalledTwice_Throws()
    {
        var member = new PartyMemberBuilder().WithStats(attack: 15, defense: 8).Build();
        var party = new PartyState([member], [member], new PartyRules(MaxActiveMembers: 4));
        var battle = BattleState.Start(party, [Enemy("Goblin", maxHp: 10)], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new AttackCommand(battle.LivingOn(BattleSide.Enemies)[0]));
        var table = new ExperienceTable([1000]);
        battle.GrantVictoryExperience(table);

        Assert.Throws<InvalidOperationException>(() => battle.GrantVictoryExperience(table));
        Assert.Equal(40, member.Experience);
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
