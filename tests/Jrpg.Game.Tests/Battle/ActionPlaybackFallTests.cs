using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// When the fallen fade: from the start of the beat that felled them, on a defeat beat after it.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackFallTests
{
    private readonly Combatant _sarxian;
    private readonly Combatant _goblin;

    public ActionPlaybackFallTests()
    {
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
        (_sarxian, _goblin) = (battle.Combatants[0], battle.Combatants[1]);
    }

    [Fact]
    public void Start_TargetFell_AddsDefeatBeat()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 40),
            new CombatantDefeated(_goblin),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Defeat], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(TimeSpan.FromSeconds(2), playback.Duration);
    }

    // The spell fells the goblin, then the backfire fells the caster: the caster stays lit through the Backfire beat
    // (1.0 to 2.0 s after the impact's defeat) and fades on a defeat beat of its own after it.
    [Fact]
    public void Start_BackfireFellsCaster_CasterFadesAfterTheBackfireCue()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 40),
            new CombatantDefeated(_goblin),
            new Backfired(_sarxian, "instability", 16),
            new CombatantDefeated(_sarxian),
        ];
        var playback = Start(events);

        playback.Update(TimeSpan.FromSeconds(2.5));

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Defeat, PlaybackBeatKind.Backfire, PlaybackBeatKind.Defeat], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(1f, playback.FallOpacityOf(_sarxian));
        Assert.Equal(0f, playback.FallOpacityOf(_goblin));
        Assert.Equal(TimeSpan.FromSeconds(4), playback.Duration);
    }

    // The poison tick (1.0 to 1.8 s) fells the attacker, who stays lit through it and fades on a defeat beat after it.
    [Fact]
    public void Start_PoisonTickFells_AddsDefeatBeat()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new StatusTicked(_sarxian, StatusKind.Poison, 9),
            new CombatantDefeated(_sarxian),
        ];
        var playback = Start(events);

        playback.Update(BattlePacing.ImpactHold + BattlePacing.TickHold);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Tick, PlaybackBeatKind.Defeat], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(new FloatingNumber(_sarxian, "9", EffectKind.Damage, BattlePacing.ImpactHold), playback.Numbers[^1]);
        Assert.Equal(1f, playback.FallOpacityOf(_sarxian));
        Assert.Equal(BattlePacing.ImpactHold + BattlePacing.DefeatHold, playback.Duration);
    }

    [Fact]
    public void Start_TwoTargetsFell_HoldsOneImpactAndOneDefeat()
    {
        var second = Combatant.FromEnemy(BattleScenarios.Catalog.Enemies["goblin"]);
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new SkillCommand(BattleScenarios.Catalog.Skills["fireball"], [_goblin, second])),
            new EffectApplied(_goblin, EffectKind.Damage, 40),
            new CombatantDefeated(_goblin),
            new EffectApplied(second, EffectKind.Damage, 40),
            new CombatantDefeated(second),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Banner, PlaybackBeatKind.Impact, PlaybackBeatKind.Defeat], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(TimeSpan.FromSeconds(2), playback.Duration);
    }

    // The hero's regen ticks (1.0 to 1.8 s), then the stunned goblin loses its turn (to 2.6 s) and its poison fells it
    // (to 3.4 s): its number and fade come after its "Stunned" cue.
    [Fact]
    public void Start_PoisonFellsSkippedHolder_TicksAndFadesAfterTheSkip()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new StatusTicked(_sarxian, StatusKind.Regen, 5),
            new TurnSkipped(_goblin, StatusKind.Stun),
            new StatusTicked(_goblin, StatusKind.Poison, 9),
            new CombatantDefeated(_goblin),
        ];
        var playback = Start(events);

        playback.Update(TimeSpan.FromSeconds(3.4));

        PlaybackBeatKind[] beats =
            [PlaybackBeatKind.Impact, PlaybackBeatKind.Tick, PlaybackBeatKind.Skip, PlaybackBeatKind.Tick, PlaybackBeatKind.Defeat];
        Assert.Equal(beats, playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(new FloatingNumber(_goblin, "9", EffectKind.Damage, TimeSpan.FromSeconds(2.6)), playback.Numbers[^1]);
        Assert.Equal(1f, playback.FallOpacityOf(_goblin));
    }

    // The fallen stay lit white for 1.0 s, then fade out over 1.0 s.
    [Fact]
    public void Update_FallenEnemy_FadesAfterTint()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 40), new CombatantDefeated(_goblin)]);
        playback.Update(TimeSpan.FromSeconds(0.9));
        var duringTint = playback.FallOpacityOf(_goblin);

        playback.Update(TimeSpan.FromSeconds(0.6));

        Assert.Equal(1, duringTint);
        Assert.Equal(0.75f, playback.FallOpacityOf(_goblin) ?? -1, 3); // Halfway through the fade, eased in: 1 - 0.5².
        playback.Update(TimeSpan.FromSeconds(0.5));
        Assert.Equal(0, playback.FallOpacityOf(_goblin));
        Assert.Null(playback.FallOpacityOf(_sarxian));
    }

    private static ActionPlayback Start(IReadOnlyList<BattleEvent> events) => ActionPlayback.Start(events, RepositoryScreenText.Text.Messages);
}
