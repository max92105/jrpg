using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Phases are entered at the end of Execute, latched (decisions 404, 405; ADR 0034). The boss has 100 HP; with two
// combatants the shuffle 1 keeps the listed order, the member first, and 0 swaps it, the boss first.
public sealed class EnemyPhaseTests
{
    private static readonly SkillDefinition _crush = new()
    {
        Id = "crush",
        Name = "Crush",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 10, Targeting.AllEnemies),
    };

    private static readonly EnemyPhase _below75 = Phase(75);
    private static readonly EnemyPhase _below50 = Phase(50);

    [Fact]
    public void Execute_HitTakesBossBelowThreshold_ReportsPhaseEnteredLast()
    {
        var boss = Boss(_below50);
        var battle = MemberFirst(attack: 51, boss);

        var events = battle.Execute(new AttackCommand(boss));

        Assert.Equal(new EnemyPhaseEntered(boss, _below50), events[^1]);
        Assert.Same(_below50, boss.EnemyPhase);
    }

    [Fact]
    public void Execute_HitLeavesBossAtTheThreshold_EntersNoPhase()
    {
        var boss = Boss(_below50);
        var battle = MemberFirst(attack: 50, boss);

        var events = battle.Execute(new AttackCommand(boss));

        Assert.Empty(events.OfType<EnemyPhaseEntered>());
        Assert.Null(boss.EnemyPhase);
    }

    [Fact]
    public void Execute_HitPastTwoThresholds_EntersTheDeeperOnceWithOneEvent()
    {
        var boss = Boss(_below75, _below50);
        var battle = MemberFirst(attack: 60, boss);

        var events = battle.Execute(new AttackCommand(boss));

        Assert.Equal(new EnemyPhaseEntered(boss, _below50), Assert.Single(events.OfType<EnemyPhaseEntered>()));
        Assert.Same(_below50, boss.EnemyPhase);
    }

    [Fact]
    public void Execute_LaterHitPastADeeperThreshold_EntersItToo()
    {
        var boss = Boss(_below75, _below50);
        var battle = MemberFirst(attack: 30, boss);
        battle.Execute(new AttackCommand(boss));
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new AttackCommand(boss));

        Assert.Equal(new EnemyPhaseEntered(boss, _below50), Assert.Single(events.OfType<EnemyPhaseEntered>()));
    }

    [Fact]
    public void Execute_BossAlreadyInThePhase_ReportsNothingMore()
    {
        var boss = Boss(_below50);
        var battle = MemberFirst(attack: 20, boss);
        boss.Vitals.TakeDamage(35);
        battle.Execute(new AttackCommand(boss));
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new AttackCommand(boss));

        Assert.Empty(events.OfType<EnemyPhaseEntered>());
        Assert.Same(_below50, boss.EnemyPhase);
    }

    [Fact]
    public void Execute_BossHealedAboveThreshold_StaysInThePhase()
    {
        var boss = Boss(_below50);
        var battle = MemberFirst(attack: 51, boss);
        battle.Execute(new AttackCommand(boss));
        boss.Vitals.RestoreHp(40);

        var events = battle.Execute(new DefendCommand());

        Assert.Empty(events.OfType<EnemyPhaseEntered>());
        Assert.Same(_below50, boss.EnemyPhase);
    }

    [Fact]
    public void Execute_PoisonTickTakesBossBelow_EntersThePhase()
    {
        var boss = Boss(_below50);
        boss.Vitals.TakeDamage(45);
        boss.Statuses.Apply(new StatusGrant(StatusKind.Poison, Power: 10, Turns: 2));
        var battle = new BattleState([Member(attack: 10), boss], new Inventory(), new FixedSequenceRandom(0));

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(new EnemyPhaseEntered(boss, _below50), events[^1]);
    }

    [Fact]
    public void Execute_StunnedBossTickTakesItBelow_PhaseEventComesAfterTheSkipAndTick()
    {
        var boss = Boss(_below50);
        boss.Vitals.TakeDamage(45);
        boss.Statuses.Apply(new StatusGrant(StatusKind.Poison, Power: 10, Turns: 2));
        boss.Statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 100, Turns: 2));
        var battle = MemberFirst(attack: 1, boss);

        var events = battle.Execute(new DefendCommand()).ToList();

        var skipped = events.IndexOf(new TurnSkipped(boss, StatusKind.Stun));
        var ticked = events.FindIndex(happened => happened is StatusTicked);
        Assert.InRange(skipped, 0, ticked);
        Assert.Equal(new EnemyPhaseEntered(boss, _below50), events[^1]);
    }

    [Fact]
    public void Execute_BossFallsToTheHit_EntersNoPhase()
    {
        var boss = Boss(_below50);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
        var battle = new BattleState([Member(attack: 100), boss, goblin], new Inventory(), new FixedSequenceRandom(2, 1));

        var events = battle.Execute(new AttackCommand(boss));

        Assert.Empty(events.OfType<EnemyPhaseEntered>());
        Assert.Null(boss.EnemyPhase);
    }

    // The member's own poison tick ends the battle in defeat with the boss standing under the threshold.
    [Fact]
    public void Execute_LastMemberHitsBossBelowThenFallsToPoison_EntersNoPhase()
    {
        var boss = Boss(_below50);
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().WithAttack(51).WithVitals(hp: 5, maxHp: 100).Build());
        member.Statuses.Apply(new StatusGrant(StatusKind.Poison, Power: 10, Turns: 2));
        var battle = new BattleState([member, boss], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new AttackCommand(boss));

        Assert.Equal(BattlePhase.Defeat, battle.Phase);
        Assert.Empty(events.OfType<EnemyPhaseEntered>());
    }

    [Fact]
    public void ChooseCommand_BossInAPhase_PicksFromThePhasesActions()
    {
        var crushing = new EnemyPhase { BelowHpPercent = 50, Tactics = [new EnemyAction { SkillId = _crush.Id, Weight = 1 }] };
        var boss = Boss(crushing);
        var battle = MemberFirst(attack: 51, boss);
        battle.Execute(new AttackCommand(boss));
        var tactics = new EnemyTactics(new FixedSequenceRandom(), new Dictionary<string, SkillDefinition> { [_crush.Id] = _crush });

        var command = tactics.ChooseCommand(battle);

        Assert.Equal(_crush, Assert.IsType<SkillCommand>(command).Skill);
    }

    [Fact]
    public void Knows_SkillNamedOnlyByAPhase_IsTrue()
    {
        var boss = Boss(new EnemyPhase { BelowHpPercent = 50, Tactics = [new EnemyAction { SkillId = _crush.Id, Weight = 1 }] });

        Assert.True(boss.Knows(_crush));
    }

    private static BattleState MemberFirst(int attack, Combatant boss) =>
        new([Member(attack), boss], new Inventory(), new FixedSequenceRandom(1));

    private static Combatant Member(int attack) => Combatant.FromPartyMember(new PartyMemberBuilder().WithAttack(attack).Build());

    private static Combatant Boss(params EnemyPhase[] phases) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("Chief").WithStats(maxHp: 100, damage: 5).WithPhases(phases).Build());

    private static EnemyPhase Phase(int belowHpPercent) => new() { BelowHpPercent = belowHpPercent, Tactics = [new EnemyAction { Weight = 1 }] };
}
