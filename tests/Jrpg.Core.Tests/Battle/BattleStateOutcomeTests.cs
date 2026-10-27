using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// How a battle ends: victory, defeat, and the experience granted after a win. The scripted shuffles: with two
// combatants, 1 keeps the listed order and 0 swaps it.
public sealed class BattleStateOutcomeTests
{
    private readonly Combatant _hero = Hero(hp: 100);
    private readonly Combatant _goblin = Goblin(hp: 40);

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
    public void GrantVictoryExperience_AfterVictory_GivesEnemyExperienceToLivingMembers()
    {
        var member = new PartyMemberBuilder().WithAttack(13).Build();
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
        var sarxian = new PartyMemberBuilder().WithAttack(13).Build();
        var jazog = new PartyMemberBuilder().WithAttack(13).Build();
        var party = new PartyState([sarxian, jazog], [sarxian, jazog], new PartyRules(MaxActiveMembers: 4));
        EnemyDefinition[] goblins = [Enemy("Goblin A", maxHp: 10), Enemy("Goblin B", maxHp: 10), Enemy("Goblin C", maxHp: 10)];
        var battle = BattleState.Start(party, goblins, new Inventory(), new FixedSequenceRandom(4, 3, 2, 1));
        BattleScript.AttackUntilOver(battle);

        var rewards = battle.GrantVictoryExperience(new ExperienceTable([1000]));

        Assert.Equal(120, rewards.Experience);
        Assert.Equal([60, 60], rewards.Gains.Select(gain => gain.Amount));
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
        var member = new PartyMemberBuilder().WithAttack(13).Build();
        var party = new PartyState([member], [member], new PartyRules(MaxActiveMembers: 4));
        var battle = BattleState.Start(party, [Enemy("Goblin", maxHp: 10)], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new AttackCommand(battle.LivingOn(BattleSide.Enemies)[0]));
        var table = new ExperienceTable([1000]);
        battle.GrantVictoryExperience(table);

        Assert.Throws<InvalidOperationException>(() => battle.GrantVictoryExperience(table));
        Assert.Equal(40, member.Experience);
    }

    private static EnemyDefinition Enemy(string name, int maxHp = 40) =>
        new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp, damage: 12).Build();

    private static Combatant Hero(int hp) => Combatant.FromPartyMember(
        new PartyMemberBuilder().WithAttack(13).WithVitals(hp, maxHp: 100).Build());

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, damage: 12).Build());
}
