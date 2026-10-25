using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A hero against one goblin (25 HP, defense 2); the shuffle (1) keeps the listed order, the hero first.
// Bash deals 15 - 2 = 13 damage and stuns for 2 turns; Jab deals 3 and stuns for 1; Brace stuns the hero for 2
// turns and changes no HP.
public sealed class BattleStatusTests
{
    private static readonly SkillDefinition _bash =
        Skill("bash", new Effect(EffectKind.Damage, 15, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2)));

    private static readonly SkillDefinition _jab =
        Skill("jab", new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 1)));

    private static readonly SkillDefinition _brace =
        Skill("brace", new Effect(EffectKind.GrantStatus, 0, Targeting.Self, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2)));

    private readonly Combatant _hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_bash, _jab, _brace).Build());

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

    [Fact]
    public void Execute_StatusOnOther_CountsDownAtEndOfHolderTurn()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);
        battle.Execute(new SkillCommand(_bash, [goblin]));

        battle.Execute(new DefendCommand());

        Assert.Equal([new ActiveStatus(StatusKind.Stun, 0, TurnsLeft: 1, IsFresh: false)], goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_TargetFalls_StatusesCleared()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);
        battle.Execute(new SkillCommand(_bash, [goblin]));
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new SkillCommand(_bash, [goblin]));

        var fall = events.ToList().IndexOf(new CombatantDefeated(goblin));
        Assert.Equal(new StatusEnded(goblin, StatusKind.Stun), events[fall + 1]);
        Assert.Empty(goblin.Statuses.Active);
    }

    [Fact]
    public void Execute_StatusRunsOut_ReportsStatusEnded()
    {
        var goblin = Goblin(hp: 25);
        var battle = Battle(goblin);
        battle.Execute(new SkillCommand(_jab, [goblin]));

        var events = battle.Execute(new DefendCommand());

        Assert.Contains(new StatusEnded(goblin, StatusKind.Stun), events);
        Assert.Empty(goblin.Statuses.Active);
    }

    private BattleState Battle(Combatant goblin) => new([_hero, goblin], new Inventory(), new FixedSequenceRandom(1));

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, attack: 20, defense: 2).Build());

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
