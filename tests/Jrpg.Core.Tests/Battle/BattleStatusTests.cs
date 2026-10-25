using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A hero against one goblin (25 HP, defense 2); the shuffle (1) keeps the listed order, the hero first.
// Bash deals 15 - 2 = 13 damage and stuns for 2 turns; Jab deals 3 and stuns for 1; Quake does the same to every
// enemy; Brace stuns the hero for 2 turns and changes no HP. A stunned combatant's turn is skipped within the Execute
// that ends the turn before it.
public sealed class BattleStatusTests
{
    private static readonly SkillDefinition _bash =
        Skill("bash", new Effect(EffectKind.Damage, 15, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2)));

    private static readonly SkillDefinition _jab =
        Skill("jab", new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 1)));

    private static readonly SkillDefinition _quake =
        Skill("quake", new Effect(EffectKind.Damage, 5, Targeting.AllEnemies, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 1)));

    private static readonly SkillDefinition _brace =
        Skill("brace", new Effect(EffectKind.GrantStatus, 0, Targeting.Self, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2)));

    private readonly Combatant _hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_bash, _jab, _quake, _brace).Build());

    [Fact]
    public void Execute_DamageWithStatus_AppliesAfterDamageAndReports()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.Equal(new EffectApplied(goblin, EffectKind.Damage, 13), events[1]);
        Assert.Equal(new StatusApplied(goblin, StatusKind.Stun, 2), events[2]);
        Assert.True(goblin.Statuses.Has(StatusKind.Stun));
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
        Assert.Contains(new StatusApplied(_hero, StatusKind.Stun, 2), events);
    }

    [Fact]
    public void Execute_SelfGrantedStatus_NotCountedDownOnCastingTurn()
    {
        var battle = Battle(Goblin(hp: 25));

        battle.Execute(new SkillCommand(_brace, [_hero]));

        Assert.Equal([new ActiveStatus(StatusKind.Stun, 0, TurnsLeft: 2, IsFresh: false)], _hero.Statuses.Active);
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
        Assert.Contains(new TurnSkipped(second, StatusKind.Stun), events);
        Assert.Equal([new ActiveStatus(StatusKind.Stun, 0, TurnsLeft: 1, IsFresh: false)], second.Statuses.Active);
    }

    [Fact]
    public void Execute_TargetFalls_StatusesCleared()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);
        battle.Execute(new SkillCommand(_bash, [goblin]));

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        var fall = events.ToList().IndexOf(new CombatantDefeated(goblin));
        Assert.Equal(new StatusEnded(goblin, StatusKind.Stun), events[fall + 1]);
        Assert.Empty(goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_NextActorStunned_SkipsTurnAndReportsIt()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        Assert.Contains(new TurnSkipped(goblin, StatusKind.Stun), events);
        Assert.Equal([new ActiveStatus(StatusKind.Stun, 0, TurnsLeft: 1, IsFresh: false)], goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_StunForOneTurn_EndsOnTheSkippedTurn()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);

        var events = battle.Execute(new SkillCommand(_jab, [goblin])).ToList();

        var skipped = events.IndexOf(new TurnSkipped(goblin, StatusKind.Stun));
        Assert.Equal(new StatusEnded(goblin, StatusKind.Stun), events[skipped + 1]);
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
            new Dictionary<string, SkillDefinition> { ["brace"] = _brace }));
        var battle = new BattleState([mage, Goblin(hp: 25)], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new SkillCommand(_brace, [mage]));

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(new TurnSkipped(mage, StatusKind.Stun), events);
        Assert.Equal(7, mage.Resources.AmountOf("mana"));
    }

    // Taunt is self-granted, so the casting turn doesn't count it down: it lasts the hero's next two turns.
    [Fact]
    public void Execute_SelfTaunt_LastsTwoOwnTurnsAfterCast()
    {
        var taunt = Skill("taunt", new Effect(EffectKind.GrantStatus, 0, Targeting.Self, new StatusGrant(StatusKind.Taunt, Power: 0, Turns: 2)));
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(taunt).Build());
        var battle = new BattleState([hero, Goblin(hp: 25)], new Inventory(), new FixedSequenceRandom(1));
        battle.Execute(new SkillCommand(taunt, [hero]));
        var turnsLeft = new List<bool>();

        for (var round = 0; round < 3; round++)
        {
            battle.Execute(new DefendCommand());
            battle.Execute(new DefendCommand());
            turnsLeft.Add(hero.Statuses.Has(StatusKind.Taunt));
        }

        Assert.Equal([true, false, false], turnsLeft);
    }

    // The list is [ally, hero, goblin]; the shuffle (2, 0) makes the turns hero, ally, goblin. Without the taunt, the
    // scripted pick (0) would be the ally, the first living member in the list.
    [Fact]
    public void ChooseCommand_AfterSelfTaunt_GoblinAttacksTheTaunter()
    {
        var taunt = Skill("taunt", new Effect(EffectKind.GrantStatus, 0, Targeting.Self, new StatusGrant(StatusKind.Taunt, Power: 0, Turns: 2)));
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithId("jazog").WithSkills(taunt).Build());
        var ally = Combatant.FromPartyMember(new PartyMemberBuilder().WithId("teelle").Build());
        var battle = new BattleState([ally, hero, Goblin(hp: 25)], new Inventory(), new FixedSequenceRandom(2, 0));
        battle.Execute(new SkillCommand(taunt, [hero]));
        battle.Execute(new DefendCommand());

        var command = new EnemyTactics(new FixedSequenceRandom(0)).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(hero), command);
    }

    private BattleState Battle(Combatant goblin) => new([_hero, goblin], new Inventory(), new FixedSequenceRandom(1));

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, attack: 20, defense: 2).Build());

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
