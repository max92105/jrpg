using System.Collections.Frozen;
using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One turn's events as the beats the screen plays (design key decision 4): a banner for a skill, an item or Defend, an
/// impact when effects land, a defeat when someone falls, a skip when a stun takes a turn. The hold lasts as long as
/// the beats; Defend and Run hold nothing. On the impact, each landed effect pops its amount and shows on its target,
/// damage as the attacker's effect strip, anything else as a placeholder cue (decision 101), and a granted status shows
/// its name, and a miss "Miss"; whoever fell shows white, then fades on the defeat beat. A lost turn shows "Stunned" on
/// its beat, and Poison and Regen pop the HP they took or restored on a tick beat. An event it doesn't know throws, so
/// a new mechanic can't be shown silently wrong (ADR 0004).
/// </summary>
internal sealed class ActionPlayback
{
    // Placeholder timing: the fallen stay lit white for 1.0 s, then fade out over 1.0 s, easing in.
    private static readonly TimeSpan _fallTint = TimeSpan.FromSeconds(1.0);
    private static readonly TimeSpan _fallFade = TimeSpan.FromSeconds(1.0);

    // Panels, tags and spots read live state at the next stage change (ADR 0013), so these need no beat of their own.
    private static readonly FrozenSet<Type> _shownByLiveState =
        [typeof(ResourceChanged), typeof(StatusEnded), typeof(PlacesSwapped), typeof(ExtraTurnReadied), typeof(BattleEnded)];

    private readonly List<PlaybackBeat> _beats = [];
    private readonly List<FloatingNumber> _numbers = [];
    private readonly List<EffectOnTarget> _strips = [];
    private readonly List<CueOnTarget> _cues = [];
    private readonly List<(Combatant Fallen, TimeSpan FadeStart)> _fallen = [];
    private string? _attackEffectFile;

    private ActionPlayback()
    {
    }

    public IReadOnlyList<PlaybackBeat> Beats => _beats;

    /// <summary>What the banner says for this action; null when it shows nothing.</summary>
    public string? BannerText { get; private set; }

    /// <summary>How long the screen holds for the beats.</summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>One number per landed effect, built once; they all pop at the impact, which comes first.</summary>
    public IReadOnlyList<FloatingNumber> Numbers => _numbers;

    /// <summary>The attacker's effect strip on each target damage landed on.</summary>
    public IReadOnlyList<EffectOnTarget> Strips => _strips;

    /// <summary>The action's name over each target an effect without an animation landed on.</summary>
    public IReadOnlyList<CueOnTarget> Cues => _cues;

    /// <summary>Time since the action landed; it runs on after the hold, while the numbers, effects and the fallen finish.</summary>
    public TimeSpan Elapsed { get; private set; }

    /// <param name="events">One turn's events, as <see cref="BattleState.Execute"/> returned them.</param>
    /// <param name="messages">How "{name} defends." is written.</param>
    /// <exception cref="InvalidOperationException">An event or a command the battle screen can't show yet.</exception>
    public static ActionPlayback Start(IReadOnlyList<BattleEvent> events, UiMessages messages)
    {
        var playback = new ActionPlayback();
        foreach (var battleEvent in events.Where(shown => !_shownByLiveState.Contains(shown.GetType())))
        {
            playback.Read(battleEvent, messages);
        }

        playback.Duration = playback._beats.Aggregate(TimeSpan.Zero, (total, beat) => total + beat.Duration);
        return playback;
    }

    public void Update(TimeSpan delta) => Elapsed += delta;

    /// <returns>How visible <paramref name="combatant"/> still is while falling, 1 to 0; null when they didn't fall here.</returns>
    public float? FallOpacityOf(Combatant combatant)
    {
        var index = _fallen.FindIndex(fall => fall.Fallen == combatant);
        if (index < 0)
        {
            return null;
        }

        var fadeShare = Easing.ShareOf(Elapsed - _fallen[index].FadeStart - _fallTint, _fallFade);
        return 1 - (fadeShare * fadeShare);
    }

    private static string? BannerTextFor(ActionStarted started, UiMessages messages) => started.Command switch
    {
        SkillCommand skill => skill.Skill.Name,
        ItemCommand item => item.Item.Name,
        DefendCommand => messages.Format(MessageIds.BattleDefends, new MessageArgument("name", started.Actor.Name)),
        MoveCommand => messages.Text(MessageIds.BattleMove),
        AttackCommand or FleeCommand => null,
        _ => throw new InvalidOperationException($"The battle screen can't show a {started.Command.GetType().Name} yet."),
    };

    private void Read(BattleEvent battleEvent, UiMessages messages)
    {
        switch (battleEvent)
        {
            case ActionStarted started:
                AddBanner(started, messages);
                break;
            case EffectApplied effect:
                AddBeatOnce(PlaybackBeatKind.Impact, BattlePacing.ImpactHold);
                AddLanding(effect);
                break;
            case CombatantDefeated defeated:
                AddFall(defeated.Combatant);
                break;
            case StatusApplied applied:
                AddBeatOnce(PlaybackBeatKind.Impact, BattlePacing.ImpactHold);
                _cues.Add(new CueOnTarget(applied.Target, messages.Text(StatusNames.IdOf(applied.Status))));
                break;
            case HitMissed missed:
                AddBeatOnce(PlaybackBeatKind.Impact, BattlePacing.ImpactHold);
                _cues.Add(new CueOnTarget(missed.Target, messages.Text(MessageIds.BattleMiss)));
                break;
            case TurnSkipped skipped:
                AddSkipCue(skipped.Combatant, messages);
                break;
            case Backfired backfired:
                AddBackfire(backfired, messages);
                break;
            case StatusTicked ticked:
                AddTick(ticked);
                break;
            default:
                throw new InvalidOperationException($"The battle screen can't show a {battleEvent.GetType().Name} yet.");
        }
    }

    private static FloatingNumberTone ToneOf(EffectKind kind) => kind switch
    {
        EffectKind.Damage => FloatingNumberTone.Damage,
        EffectKind.RestoreHp or EffectKind.Revive => FloatingNumberTone.Heal,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "An effect with no amount to show."),
    };

    // Only damage has an animation so far: the attacker's strip (decision 101). Anything else gets a cue with the action's name.
    // An action without one, an attack, only ever lands damage.
    private void AddLanding(EffectApplied effect)
    {
        var stack = _numbers.Count(number => number.Target == effect.Target && number.ShowsAt == TimeSpan.Zero);
        var affinity = stack == 0 ? effect.Affinity : HitAffinity.Neutral;
        _numbers.Add(new FloatingNumber(effect.Target, NumberText.Of(effect.Amount), ToneOf(effect.Kind), Stack: stack, Affinity: affinity));
        if (effect.Kind == EffectKind.Damage && _attackEffectFile is { } file)
        {
            // A multi-hit plays one strip on its target, as its hits make one impact.
            if (stack == 0)
            {
                _strips.Add(new EffectOnTarget(effect.Target, file));
            }
        }
        else if (BannerText is { } actionName)
        {
            _cues.Add(new CueOnTarget(effect.Target, actionName));
        }
    }

    private void AddBanner(ActionStarted started, UiMessages messages)
    {
        BannerText = BannerTextFor(started, messages);
        _attackEffectFile = BattlerArt.Of(started.Actor).AttackEffectFile;
        if (BannerText is not null)
        {
            AddBeatOnce(PlaybackBeatKind.Banner, TimeSpan.Zero);
        }
    }

    // A fall fades from the start of the beat that caused it: the impact, or a backfire or a tick after it. Those two
    // give their fall its own defeat beat, so the fallen fade after the cue or the number rather than during it.
    private void AddFall(Combatant fallen)
    {
        var causeIndex = _beats.FindLastIndex(beat => beat.Kind != PlaybackBeatKind.Defeat);
        if (causeIndex >= 0 && _beats[causeIndex].Kind is PlaybackBeatKind.Backfire or PlaybackBeatKind.Tick)
        {
            _fallen.Add((fallen, StartOfBeatAt(causeIndex)));
            _beats.Add(new PlaybackBeat(PlaybackBeatKind.Defeat, BattlePacing.DefeatHold - _beats[causeIndex].Duration));
            return;
        }

        _fallen.Add((fallen, TimeSpan.Zero));
        AddBeatOnce(PlaybackBeatKind.Defeat, BattlePacing.DefeatHold - BattlePacing.ImpactHold);
    }

    // When a beat of this kind starts, or would start if added now: the beats before the first one of it.
    private TimeSpan StartOf(PlaybackBeatKind kind) =>
        _beats.TakeWhile(beat => beat.Kind != kind).Aggregate(TimeSpan.Zero, (total, beat) => total + beat.Duration);

    private TimeSpan StartOfBeatAt(int index) => _beats.Take(index).Aggregate(TimeSpan.Zero, (total, beat) => total + beat.Duration);

    // The backfire has its own beat after the impact; its cue and the HP it cost show when that beat starts.
    private void AddBackfire(Backfired backfired, UiMessages messages)
    {
        var start = StartOf(PlaybackBeatKind.Backfire);
        AddBeatOnce(PlaybackBeatKind.Backfire, BattlePacing.BackfireHold);
        _cues.Add(new CueOnTarget(backfired.Caster, messages.Text(MessageIds.BattleBackfire), start));
        _numbers.Add(new FloatingNumber(backfired.Caster, NumberText.Of(backfired.HpLost), FloatingNumberTone.Damage, start));
    }

    // One holder's ticks share a beat after the beat before them; a stunned holder's come after its "Stunned" cue, so
    // they get a Tick beat of their own. Each number shows when its beat starts.
    private void AddTick(StatusTicked ticked)
    {
        if (_beats.FindLast(beat => beat.Kind != PlaybackBeatKind.Defeat)?.Kind != PlaybackBeatKind.Tick)
        {
            _beats.Add(new PlaybackBeat(PlaybackBeatKind.Tick, BattlePacing.TickHold));
        }

        var start = StartOfBeatAt(_beats.FindLastIndex(beat => beat.Kind == PlaybackBeatKind.Tick));
        var tone = ticked.Status == StatusKind.Poison ? FloatingNumberTone.Poison : FloatingNumberTone.Heal;
        _numbers.Add(new FloatingNumber(ticked.Holder, NumberText.Of(ticked.Amount), tone, start));
    }

    // Every lost turn in one Execute shares one beat after the action's own, and each cue shows when it starts.
    private void AddSkipCue(Combatant combatant, UiMessages messages)
    {
        var start = StartOf(PlaybackBeatKind.Skip);
        AddBeatOnce(PlaybackBeatKind.Skip, BattlePacing.SkipHold);
        _cues.Add(new CueOnTarget(combatant, messages.Text(MessageIds.BattleTurnSkipped), start));
    }

    // Effects landing together make one impact, and any falls one defeat, however many targets there are.
    private void AddBeatOnce(PlaybackBeatKind kind, TimeSpan duration)
    {
        if (!_beats.Exists(beat => beat.Kind == kind))
        {
            _beats.Add(new PlaybackBeat(kind, duration));
        }
    }
}
