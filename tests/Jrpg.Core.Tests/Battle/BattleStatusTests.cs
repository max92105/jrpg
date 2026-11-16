using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A hero against one goblin (25 HP); the shuffle (1) keeps the listed order, the hero first. Bash deals 13 damage and
// stuns for 2 turns; Jab deals 3 and stuns for 1; Quake does the same to every enemy; Brace stuns the hero for 2 turns
// and changes no HP. A stunned combatant's turn is skipped within the Execute that ends the turn before it.
public sealed class BattleStatusTests
{
    private static readonly SkillDefinition _bash =
        Skill("bash", TestStatuses.Granting(new Effect(EffectKind.Damage, 13, Targeting.SingleEnemy), TestStatuses.Stun(100, 2)));

    private static readonly SkillDefinition _jab =
        Skill("jab", TestStatuses.Granting(new Effect(EffectKind.Damage, 3, Targeting.SingleEnemy), TestStatuses.Stun(100, 1)));

    private static readonly SkillDefinition _quake =
        Skill("quake", TestStatuses.Granting(new Effect(EffectKind.Damage, 3, Targeting.AllEnemies), TestStatuses.Stun(100, 1)));

    private static readonly SkillDefinition _brace =
        Skill("brace", TestStatuses.Granting(new Effect(EffectKind.GrantStatus, 0, Targeting.Self), TestStatuses.Stun(100, 2)));

    private readonly Combatant _hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_bash, _jab, _quake, _brace).Build());

    [Fact]
    public void Execute_DamageWithStatus_AppliesAfterDamageAndReports()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.Equal(new EffectApplied(goblin, EffectKind.Damage, 13), events[1]);
        var applied = Assert.IsType<StatusApplied>(events[2]);
        Assert.Equal((goblin, TestStatuses.StunId, 2), (applied.Target, TestStatuses.IdOf(applied.Status), applied.Turns));
        Assert.True(goblin.Statuses.Has(TestStatuses.StunId));
    }

    [Fact]
    public void Execute_StatusOnFallenTarget_GrantsNothing()
    {
        var goblin = Goblin(hp: 10);
        var battle = Battle(goblin);

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.DoesNotContain(events, battleEvent => battleEvent is StatusApplied);
        Assert.Empty(goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_GrantStatusEffect_ReportsNoEffectApplied()
    {
        var battle = Battle(Goblin(hp: 25));

        var events = battle.Execute(new SkillCommand(_brace, [_hero]));

        Assert.DoesNotContain(events, battleEvent => battleEvent is EffectApplied);
        Assert.Contains(
            events.OfType<StatusApplied>(),
            applied => (applied.Target, TestStatuses.IdOf(applied.Status), applied.Turns) == (_hero, TestStatuses.StunId, 2));
    }

    [Fact]
    public void Execute_SelfGrantedStatus_NotCountedDownOnCastingTurn()
    {
        var battle = Battle(Goblin(hp: 25));

        battle.Execute(new SkillCommand(_brace, [_hero]));

        var status = Assert.Single(_hero.Statuses.Active);
        Assert.Equal(
            (TestStatuses.StunId, 0, 2, false),
            (TestStatuses.IdOf(status), TestStatuses.AmountOf(status), status.TurnsLeft, status.IsFresh));
    }

    // The second goblin is stunned on the hero's turn; the first goblin's turn in between doesn't count it down.
    [Fact]
    public void Execute_StatusOnOther_CountsDownAtEndOfHolderTurn()
    {
        var (first, second) = (Goblin(hp: 25), Goblin(hp: 25));
        var battle = new BattleState([_hero, first, second], new Inventory(), new FixedSequenceRandom(2, 1));
        battle.Execute(new SkillCommand(_bash, [second]));
        var turnsBeforeHolderTurn = second.Statuses.Active[0].TurnsLeft;

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(2, turnsBeforeHolderTurn);
        Assert.Contains(
            events.OfType<TurnSkipped>(),
            skip => (skip.Combatant, TestStatuses.IdOf(skip.Cause)) == (second, TestStatuses.StunId));
        var status = Assert.Single(second.Statuses.Active);
        Assert.Equal(
            (TestStatuses.StunId, 0, 1, false),
            (TestStatuses.IdOf(status), TestStatuses.AmountOf(status), status.TurnsLeft, status.IsFresh));
    }

    [Fact]
    public void Execute_TargetFalls_StatusesCleared()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);
        battle.Execute(new SkillCommand(_bash, [goblin]));

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        var fall = events.ToList().IndexOf(new CombatantDefeated(goblin));
        var ended = Assert.IsType<StatusEnded>(events[fall + 1]);
        Assert.Equal((goblin, TestStatuses.StunId), (ended.Holder, TestStatuses.IdOf(ended.Status)));
        Assert.Empty(goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_NextActorStunned_SkipsTurnAndReportsIt()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.Contains(
            events.OfType<TurnSkipped>(),
            skip => (skip.Combatant, TestStatuses.IdOf(skip.Cause)) == (goblin, TestStatuses.StunId));
        var status = Assert.Single(goblin.Statuses.Active);
        Assert.Equal(
            (TestStatuses.StunId, 0, 1, false),
            (TestStatuses.IdOf(status), TestStatuses.AmountOf(status), status.TurnsLeft, status.IsFresh));
    }

    [Fact]
    public void Execute_StunForOneTurn_EndsOnTheSkippedTurn()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);

        var events = battle.Execute(new SkillCommand(_jab, [goblin])).ToList();

        var skipped = events.FindIndex(battleEvent =>
            battleEvent is TurnSkipped skip && (skip.Combatant, TestStatuses.IdOf(skip.Cause)) == (goblin, TestStatuses.StunId));
        var ended = Assert.IsType<StatusEnded>(events[skipped + 1]);
        Assert.Equal((goblin, TestStatuses.StunId), (ended.Holder, TestStatuses.IdOf(ended.Status)));
        Assert.Empty(goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_TwoStunnedInARow_SkipsBoth()
    {
        var (first, second) = (Goblin(hp: 25), Goblin(hp: 25));
        var battle = new BattleState([_hero, first, second], new Inventory(), new FixedSequenceRandom(2, 1));

        var events = battle.Execute(new SkillCommand(_quake, [first, second]));

        Assert.Equal([first, second], events.OfType<TurnSkipped>().Select(skip => skip.Combatant));
        Assert.Same(_hero, battle.CurrentActor);
    }

    [Fact]
    public void Execute_StunnedSkipped_CurrentActorIsTheOneAfter()
    {
        var (first, second) = (Goblin(hp: 25), Goblin(hp: 25));
        var battle = new BattleState([_hero, first, second], new Inventory(), new FixedSequenceRandom(2, 1));

        battle.Execute(new SkillCommand(_jab, [first]));

        Assert.Same(second, battle.CurrentActor);
        Assert.Equal(BattlePhase.EnemyTurn, battle.Phase);
    }

    [Fact]
    public void Execute_OnlyEnemyStunnedBeforeMember_PhaseIsPartyTurn()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);

        battle.Execute(new SkillCommand(_jab, [goblin]));

        Assert.Same(_hero, battle.CurrentActor);
        Assert.Equal(BattlePhase.PartyTurn, battle.Phase);
    }

    // A lost turn is still a turn: the stunned mage gains mana on it.
    [Fact]
    public void Execute_StunnedActorsUpkeep_GainsPerTurn()
    {
        var mana = new ResourceDefinition("mana", "Mana", Max: 10, StartsAt: 3) { Gains = new ResourceGains { PerTurn = 2 } };
        var mage = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("omer").BuildDefinition(),
            new ClassBuilder().WithId("mage").WithResources(mana).WithSkills("brace").Build(),
            new Dictionary<string, SkillDefinition> { ["brace"] = _brace },
            TestStatRules.Neutral));
        var battle = new BattleState([mage, Goblin(hp: 25)], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new SkillCommand(_brace, [mage]));

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(
            events.OfType<TurnSkipped>(),
            skip => (skip.Combatant, TestStatuses.IdOf(skip.Cause)) == (mage, TestStatuses.StunId));
        Assert.Equal(7, mage.Resources.AmountOf("mana"));
    }

    // Taunt is self-granted, so the casting turn doesn't count it down: it lasts the hero's next two turns.
    [Fact]
    public void Execute_SelfTaunt_LastsTwoOwnTurnsAfterCast()
    {
        var taunt = Skill(
            "taunt", TestStatuses.Granting(new Effect(EffectKind.GrantStatus, 0, Targeting.Self), TestStatuses.Taunt(2)));
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(taunt).Build());
        var battle = new BattleState([hero, Goblin(hp: 25)], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new SkillCommand(taunt, [hero]));
        var turnsLeft = new List<bool>();

        for (var round = 0; round < 3; round++)
        {
            battle.Execute(new DefendCommand());
            battle.Execute(new DefendCommand());
            turnsLeft.Add(hero.Statuses.Has(TestStatuses.TauntId));
        }

        Assert.Equal([true, false, false], turnsLeft);
    }

    // The list is [ally, hero, goblin]; the shuffle (2, 0) makes the turns hero, ally, goblin. Without the taunt, the
    // scripted pick (0) would be the ally, the first living member in the list.
    [Fact]
    public void ChooseCommand_AfterSelfTaunt_GoblinAttacksTheTaunter()
    {
        var taunt = Skill(
            "taunt", TestStatuses.Granting(new Effect(EffectKind.GrantStatus, 0, Targeting.Self), TestStatuses.Taunt(2)));
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithId("jazog").WithSkills(taunt).Build());
        var ally = Combatant.FromPartyMember(new PartyMemberBuilder().WithId("teelle").Build());
        var battle = new BattleState([ally, hero, Goblin(hp: 25)], new Inventory(), new FixedSequenceRandom(2, 0));
        battle.Execute(new SkillCommand(taunt, [hero]));
        battle.Execute(new DefendCommand());

        var command = new EnemyTactics(new FixedSequenceRandom(0), new Dictionary<string, SkillDefinition>()).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(hero), command);
    }

    // The hero hits for 13; on an Exposed (+50 %) goblin that lands as 19, rounded down.
    [Fact]
    public void Execute_AttackOnExposed_DealsOneAndAHalf()
    {
        var goblin = Goblin(hp: 40);
        goblin.Statuses.Apply(TestStatuses.Exposed(50, 3));
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithAttack(13).Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(new EffectApplied(goblin, EffectKind.Damage, 19), events[1]);
    }

    private BattleState Battle(Combatant goblin) => new([_hero, goblin], new Inventory(), new FixedSequenceRandom(1));

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, damage: 20).Build());

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
