using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One turn's events as the beats the screen plays (design key decision 4): a banner for a skill, an item or Defend, an
/// impact when effects land, a defeat when someone falls. The hold lasts as long as the beats; Defend and Run hold
/// nothing. On the impact, each landed effect pops its amount and shows on its target, damage as the attacker's effect
/// strip, anything else as a placeholder cue (decision 101); whoever fell shows white, then fades on the defeat beat.
/// An event it doesn't know throws, so a new mechanic can't be shown silently wrong (ADR 0004).
/// </summary>
internal sealed class ActionPlayback
{
    // Placeholder timing: the fallen stay lit white for 1.0 s, then fade out over 1.0 s, easing in.
    private static readonly TimeSpan _fallTint = TimeSpan.FromSeconds(1.0);
    private static readonly TimeSpan _fallFade = TimeSpan.FromSeconds(1.0);

    private readonly List<PlaybackBeat> _beats = [];
    private readonly List<FloatingNumber> _numbers = [];
    private readonly List<EffectOnTarget> _strips = [];
    private readonly List<CueOnTarget> _cues = [];
    private readonly List<Combatant> _fallen = [];
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
        foreach (var battleEvent in events)
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
        if (!_fallen.Contains(combatant))
        {
            return null;
        }

        var fadeShare = Math.Clamp((float)((Elapsed - _fallTint) / _fallFade), 0, 1);
        return 1 - (fadeShare * fadeShare);
    }

    private static string? BannerTextFor(ActionStarted started, UiMessages messages) => started.Command switch
    {
        SkillCommand skill => skill.Skill.Name,
        ItemCommand item => item.Item.Name,
        DefendCommand => messages.Format(MessageIds.BattleDefends, new MessageArgument("name", started.Actor.Name)),
        AttackCommand or FleeCommand => null,
        _ => throw new InvalidOperationException($"The battle screen can't show a {started.Command.GetType().Name} yet."),
    };

    private void Read(BattleEvent battleEvent, UiMessages messages)
    {
        switch (battleEvent)
        {
            case ActionStarted started:
                BannerText = BannerTextFor(started, messages);
                _attackEffectFile = BattlerArt.Of(started.Actor).AttackEffectFile;
                if (BannerText is not null)
                {
                    AddBeatOnce(PlaybackBeatKind.Banner, TimeSpan.Zero);
                }

                break;
            case EffectApplied effect:
                AddBeatOnce(PlaybackBeatKind.Impact, BattlePacing.ImpactHold);
                AddLanding(effect);
                break;
            case CombatantDefeated defeated:
                AddBeatOnce(PlaybackBeatKind.Defeat, BattlePacing.DefeatHold - BattlePacing.ImpactHold);
                _fallen.Add(defeated.Combatant);
                break;
            case BattleEnded:
                break;
            default:
                throw new InvalidOperationException($"The battle screen can't show a {battleEvent.GetType().Name} yet.");
        }
    }

    // Only damage has an animation so far: the attacker's strip (decision 101). Anything else gets a cue with the action's name.
    // An action without one, an attack, only ever lands damage.
    private void AddLanding(EffectApplied effect)
    {
        _numbers.Add(new FloatingNumber(effect.Target, NumberText.Of(effect.Amount), effect.Kind));
        if (effect.Kind == EffectKind.Damage && _attackEffectFile is { } file)
        {
            _strips.Add(new EffectOnTarget(effect.Target, file));
        }
        else if (BannerText is { } actionName)
        {
            _cues.Add(new CueOnTarget(effect.Target, actionName));
        }
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
