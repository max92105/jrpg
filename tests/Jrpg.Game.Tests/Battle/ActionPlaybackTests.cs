using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackTests
{
    private readonly Combatant _sarxian;
    private readonly Combatant _goblin;

    public ActionPlaybackTests()
    {
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
        (_sarxian, _goblin) = (battle.Combatants[0], battle.Combatants[1]);
    }

    [Fact]
    public void Start_AttackLanded_HasOneImpactBeat()
    {
        BattleEvent[] events = [new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)];

        var playback = Start(events);

        Assert.Equal([new PlaybackBeat(PlaybackBeatKind.Impact, BattlePacing.ImpactHold)], playback.Beats);
    }

    [Fact]
    public void Start_ResourceChanged_AddsNoBeat()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new ResourceChanged(_sarxian, "mana", 2)]);

        Assert.Empty(playback.Beats);
        Assert.Equal(TimeSpan.Zero, playback.Duration);
    }

    [Fact]
    public void Start_StatusApplied_AddsCueWithStatusName()
    {
        var playback = Start(
            [new ActionStarted(_sarxian, new AttackCommand(_goblin)), new StatusApplied(_goblin, TestStatuses.Stun, 1)]);

        Assert.Equal([new CueOnTarget(_goblin, "Stun")], playback.Cues);
        Assert.Equal([PlaybackBeatKind.Impact], playback.Beats.Select(beat => beat.Kind));
    }

    [Fact]
    public void Start_HitMissed_AddsMissCueOnTheImpact()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new HitMissed(_goblin)]);

        Assert.Equal([new CueOnTarget(_goblin, "Miss")], playback.Cues);
        Assert.Equal([PlaybackBeatKind.Impact], playback.Beats.Select(beat => beat.Kind));
        Assert.Empty(playback.Numbers);
    }

    [Fact]
    public void Start_StatusResisted_AddsResistedCueOnTheImpact()
    {
        var playback = Start(
            [new ActionStarted(_sarxian, new AttackCommand(_goblin)), new StatusResisted(_goblin, TestStatuses.Stun)]);

        Assert.Equal([new CueOnTarget(_goblin, "Resisted")], playback.Cues);
        Assert.Equal([PlaybackBeatKind.Impact], playback.Beats.Select(beat => beat.Kind));
    }

    [Fact]
    public void Start_TurnSkippedAfterAction_AddsSkipBeatAfterImpact()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new StatusApplied(_goblin, TestStatuses.Stun, 1),
            new TurnSkipped(_goblin, TestStatuses.Stun),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Skip], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(new CueOnTarget(_goblin, "Stunned", BattlePacing.ImpactHold), playback.Cues[^1]);
    }

    [Fact]
    public void Start_TwoTurnsSkipped_ShareOneSkipBeat()
    {
        var second = Combatant.FromEnemy(BattleScenarios.Catalog.Enemies["goblin"]);
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new TurnSkipped(_goblin, TestStatuses.Stun),
            new TurnSkipped(second, TestStatuses.Stun),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Skip], playback.Beats.Select(beat => beat.Kind));
        Assert.All(playback.Cues, cue => Assert.Equal(BattlePacing.ImpactHold, cue.ShowsAt));
        Assert.Equal(2, playback.Cues.Count);
    }

    [Fact]
    public void Start_StatusEnded_AddsNoBeat()
    {
        var playback = Start(
            [new ActionStarted(_sarxian, new AttackCommand(_goblin)), new StatusEnded(_goblin, TestStatuses.Stun)]);

        Assert.Empty(playback.Beats);
        Assert.Empty(playback.Cues);
    }

    [Fact]
    public void Start_Backfired_AddsBackfireBeatAfterImpact()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new Backfired(_sarxian, "instability", 16),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Backfire], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(new CueOnTarget(_sarxian, "Backfire", BattlePacing.ImpactHold), playback.Cues[^1]);
        Assert.Equal(new FloatingNumber(_sarxian, "16", FloatingNumberTone.Damage, BattlePacing.ImpactHold), playback.Numbers[^1]);
    }

    [Fact]
    public void Start_FourHitsOneTarget_FourNumbersStaggered()
    {
        var hit = new EffectApplied(_goblin, EffectKind.Damage, 6);

        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), hit, hit, hit, hit]);

        Assert.Equal([0, 1, 2, 3], playback.Numbers.Select(number => number.Stack));
        Assert.Single(playback.Strips);
        Assert.Equal([PlaybackBeatKind.Impact], playback.Beats.Select(beat => beat.Kind));
    }

    [Fact]
    public void Start_StatusTicked_AddsTickBeatAfterImpact()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new StatusTicked(_sarxian, TestStatuses.Regen, 5),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Tick], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(new FloatingNumber(_sarxian, "5", FloatingNumberTone.Heal, BattlePacing.ImpactHold), playback.Numbers[^1]);
    }

    [Fact]
    public void Start_Defend_HasNoHold()
    {
        var playback = Start([new ActionStarted(_sarxian, new DefendCommand())]);

        Assert.Equal(TimeSpan.Zero, playback.Duration);
    }

    [Fact]
    public void Start_AttackLanded_AddsDamageNumberOnTarget()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)]);

        Assert.Equal([new FloatingNumber(_goblin, "13", FloatingNumberTone.Damage)], playback.Numbers);
    }

    [Fact]
    public void Start_CriticalHit_UsesCriticalTone()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 19, IsCritical: true)]);

        Assert.Equal([new FloatingNumber(_goblin, "19", FloatingNumberTone.Critical)], playback.Numbers);
    }

    [Fact]
    public void Start_AttackLanded_PlaysAttackersStripOnTarget()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)]);

        var strip = new EffectOnTarget(_goblin, _sarxian.Member!.Definition.AttackEffectFile, IsMirrored: true);
        Assert.Equal([strip], playback.Strips);
        Assert.Empty(playback.Cues);
    }

    [Fact]
    public void Start_HealLanded_ShowsPlaceholderCueNotStrip()
    {
        var potion = BattleScenarios.Catalog.Items["potion"];

        var playback = Start([new ActionStarted(_sarxian, new ItemCommand(potion, [_sarxian])), new EffectApplied(_sarxian, EffectKind.RestoreHp, 50)]);

        Assert.Equal([new CueOnTarget(_sarxian, potion.Name)], playback.Cues);
        Assert.Empty(playback.Strips);
    }

    // BattleEvent stays open, unlike BattleCommand: new events come with new mechanics (ADR 0004), and the playback must
    // refuse what it can't show rather than show it silently wrong.
    [Fact]
    public void Start_UnknownEvent_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Start([new NotYetShownEvent()]));

        Assert.Contains(nameof(NotYetShownEvent), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_WeakHit_ShowsWeakCueByTheNumber()
    {
        BattleEvent[] events =
            [new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13, HitAffinity.Weak)];

        var playback = Start(events);

        Assert.Equal(HitAffinity.Weak, Assert.Single(playback.Numbers).Affinity);
    }

    [Fact]
    public void Read_ResistedMultiHit_CuesOnlyTheFirstNumber()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 6, HitAffinity.Resisted),
            new EffectApplied(_goblin, EffectKind.Damage, 6, HitAffinity.Resisted),
        ];

        var playback = Start(events);

        Assert.Equal([HitAffinity.Resisted, HitAffinity.Neutral], playback.Numbers.Select(number => number.Affinity));
    }

    private static ActionPlayback Start(IReadOnlyList<BattleEvent> events) => ActionPlayback.Start(events, RepositoryScreenText.Text.Messages);

    private sealed record NotYetShownEvent : BattleEvent;
}
