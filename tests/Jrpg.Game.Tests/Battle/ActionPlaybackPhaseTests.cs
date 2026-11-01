using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// A phase change shows "Enraged!" on the enemy in a beat of its own, after every beat of the action (decision 406).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackPhaseTests
{
    private static readonly EnemyPhase _phase = new() { BelowHpPercent = 50, Tactics = [new EnemyAction { Weight = 1 }] };

    private readonly Combatant _sarxian;
    private readonly Combatant _goblin;
    private readonly Combatant _nextGoblin;

    public ActionPlaybackPhaseTests()
    {
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
        (_sarxian, _goblin, _nextGoblin) = (battle.Combatants[0], battle.Combatants[1], battle.Combatants[2]);
    }

    [Fact]
    public void Start_HitEntersAPhase_CueShowsInAPhaseBeatAfterTheImpact()
    {
        var playback = Start(
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new EnemyPhaseEntered(_goblin, _phase),
        ]);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Phase], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal([new CueOnTarget(_goblin, "Enraged!", BattlePacing.ImpactHold)], playback.Cues);
        Assert.Equal(BattlePacing.ImpactHold + BattlePacing.PhaseHold, playback.Duration);
    }

    [Fact]
    public void Start_PhaseEnteredWithANextActorStunned_PhaseBeatComesAfterTheSkip()
    {
        var playback = Start(
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new TurnSkipped(_nextGoblin, StatusKind.Stun),
            new EnemyPhaseEntered(_goblin, _phase),
        ]);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Skip, PlaybackBeatKind.Phase], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(new CueOnTarget(_goblin, "Enraged!", BattlePacing.ImpactHold + BattlePacing.SkipHold), playback.Cues[^1]);
    }

    [Fact]
    public void Start_StunnedBossTickCrossesTheThreshold_PhaseBeatComesAfterTheTick()
    {
        var playback = Start(
        [
            new ActionStarted(_sarxian, new DefendCommand()),
            new TurnSkipped(_goblin, StatusKind.Stun),
            new StatusTicked(_goblin, StatusKind.Poison, 5),
            new EnemyPhaseEntered(_goblin, _phase),
        ]);

        Assert.Equal(
            [PlaybackBeatKind.Banner, PlaybackBeatKind.Skip, PlaybackBeatKind.Tick, PlaybackBeatKind.Phase],
            playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(new CueOnTarget(_goblin, "Enraged!", BattlePacing.SkipHold + BattlePacing.TickHold), playback.Cues[^1]);
        Assert.Equal(BattlePacing.SkipHold, Assert.Single(playback.Numbers).ShowsAt);
    }

    // Two enemies reaching a phase in one call share one beat, as two skipped turns do.
    [Fact]
    public void Start_TwoEnemiesEnterAPhase_ShareOnePhaseBeat()
    {
        var playback = Start(
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new EnemyPhaseEntered(_goblin, _phase),
            new EnemyPhaseEntered(_nextGoblin, _phase),
        ]);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Phase], playback.Beats.Select(beat => beat.Kind));
        Assert.All(playback.Cues, cue => Assert.Equal(BattlePacing.ImpactHold, cue.ShowsAt));
        Assert.Equal(2, playback.Cues.Count);
    }

    private static ActionPlayback Start(IReadOnlyList<BattleEvent> events) => ActionPlayback.Start(events, RepositoryScreenText.Text.Messages);
}
