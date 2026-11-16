using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A hero (100 HP) against one goblin (attack 20); the shuffle (1) keeps the hero first. The statuses are applied
// straight onto their holder, so each is fresh when its holder's turn ends: it ticks anyway.
public sealed class BattleTickingStatusTests
{
    [Fact]
    public void Execute_PoisonedActorEndsTurn_LosesPowerIgnoringResistancesAndExposed()
    {
        var (hero, _, battle) = Battle(goblinHp: 25);
        hero.Statuses.Apply(TestStatuses.Guard(50, 3));
        hero.Statuses.Apply(TestStatuses.Exposed(50, 3));
        hero.Statuses.Apply(TestStatuses.Poison(7, 3));

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(
            events.OfType<StatusTicked>(),
            ticked => (ticked.Holder, TestStatuses.IdOf(ticked.Status), ticked.Amount) == (hero, TestStatuses.PoisonId, 7));
        Assert.Equal(93, hero.Vitals.Hp);
    }

    [Fact]
    public void Execute_PoisonFellsLastEnemy_Victory()
    {
        var (hero, goblin, battle) = Battle(goblinHp: 5);
        goblin.Statuses.Apply(TestStatuses.Poison(10, 3));
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Contains(new CombatantDefeated(goblin), events);
        Assert.Equal(new BattleEnded(BattlePhase.Victory), events[^1]);
        Assert.Equal(BattlePhase.Victory, battle.Phase);
    }

    // The fall clears the poison; the mage gains no mana and no status counts down after it.
    [Fact]
    public void Execute_PoisonFellsActor_NoGainOrCountDownAfter()
    {
        var mana = new ResourceDefinition("mana", "Mana", Max: 10, StartsAt: 3) { Gains = new ResourceGains { PerTurn = 2 } };
        var mage = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("omer").BuildDefinition(),
            new ClassBuilder().WithId("mage").WithResources(mana).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral));
        mage.Vitals.TakeDamage(95);
        mage.Statuses.Apply(TestStatuses.Poison(10, 3));
        var battle = new BattleState([mage, Goblin(hp: 25)], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new DefendCommand()).ToList();

        var tick = events.FindIndex(battleEvent => battleEvent is StatusTicked ticked
            && (ticked.Holder, TestStatuses.IdOf(ticked.Status), ticked.Amount) == (mage, TestStatuses.PoisonId, 5));
        Assert.Collection(
            events[(tick + 1)..],
            battleEvent => Assert.Equal(new CombatantDefeated(mage), battleEvent),
            battleEvent =>
            {
                var ended = Assert.IsType<StatusEnded>(battleEvent);
                Assert.Equal((mage, TestStatuses.PoisonId), (ended.Holder, TestStatuses.IdOf(ended.Status)));
            },
            battleEvent => Assert.Equal(new BattleEnded(BattlePhase.Defeat), battleEvent));
        Assert.Equal(3, mage.Resources.AmountOf("mana"));
    }

    [Fact]
    public void Execute_RegenAtFullHp_ReportsZero()
    {
        var (hero, _, battle) = Battle(goblinHp: 25);
        hero.Statuses.Apply(TestStatuses.Regen(5, 3));

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(
            events.OfType<StatusTicked>(),
            ticked => (ticked.Holder, TestStatuses.IdOf(ticked.Status), ticked.Amount) == (hero, TestStatuses.RegenId, 0));
    }

    [Fact]
    public void Execute_RegenOnWounded_RestoresPower()
    {
        var (hero, _, battle) = Battle(goblinHp: 25);
        hero.Vitals.TakeDamage(20);
        hero.Statuses.Apply(TestStatuses.Regen(5, 3));

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(
            events.OfType<StatusTicked>(),
            ticked => (ticked.Holder, TestStatuses.IdOf(ticked.Status), ticked.Amount) == (hero, TestStatuses.RegenId, 5));
        Assert.Equal(85, hero.Vitals.Hp);
    }

    // Jab hits for 3 and stuns for one turn, so the goblin's next turn is skipped within the same Execute.
    [Fact]
    public void Execute_StunnedAndPoisoned_TicksOnSkippedTurn()
    {
        var jab = Skill(
            "jab", TestStatuses.Granting(new Effect(EffectKind.Damage, 3, Targeting.SingleEnemy), TestStatuses.Stun(100, 1)));
        var (_, goblin, battle) = Battle(goblinHp: 25, jab);
        goblin.Statuses.Apply(TestStatuses.Poison(4, 3));

        var events = battle.Execute(new SkillCommand(jab, [goblin])).ToList();

        var skipped = events.FindIndex(battleEvent =>
            battleEvent is TurnSkipped skip && (skip.Combatant, TestStatuses.IdOf(skip.Cause)) == (goblin, TestStatuses.StunId));
        var ticked = Assert.IsType<StatusTicked>(events[skipped + 1]);
        Assert.Equal((goblin, TestStatuses.PoisonId, 4), (ticked.Holder, TestStatuses.IdOf(ticked.Status), ticked.Amount));
        Assert.Equal(18, goblin.Vitals.Hp);
    }

    // A Ready puts the hero's extra turn right after the goblin's; it has no upkeep, so the poison doesn't tick.
    [Fact]
    public void Execute_PoisonedActorEndsExtraTurn_NoTick()
    {
        var ready = Skill("ready", new Effect(EffectKind.ReadyExtraTurn, 0, Targeting.OtherCombatant));
        var (hero, goblin, battle) = Battle(goblinHp: 25, ready);
        battle.Execute(new SkillCommand(ready, [goblin]));
        battle.Execute(new DefendCommand());
        hero.Statuses.Apply(TestStatuses.Poison(7, 3));

        var events = battle.Execute(new DefendCommand());

        Assert.DoesNotContain(events, battleEvent => battleEvent is StatusTicked);
    }

    [Fact]
    public void Execute_PoisonFellsEnemy_PaladinGainsConviction()
    {
        var conviction = new ResourceDefinition("conviction", "Conviction", Max: 10, StartsAt: 0)
        {
            Gains = new ResourceGains { PerEnemyFallen = 3 },
        };
        var paladin = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
            new ClassBuilder().WithId("paladin").WithResources(conviction).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral));
        var goblin = Goblin(hp: 5);
        goblin.Statuses.Apply(TestStatuses.Poison(10, 3));
        var battle = new BattleState([paladin, goblin], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new DefendCommand());

        battle.Execute(new AttackCommand(paladin));

        Assert.Equal(3, paladin.Resources.AmountOf("conviction"));
    }

    private static (Combatant Hero, Combatant Goblin, BattleState Battle) Battle(int goblinHp, params SkillDefinition[] skills)
    {
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(skills).Build());
        var goblin = Goblin(goblinHp);
        return (hero, goblin, new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1)));
    }

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, damage: 20).Build());

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
