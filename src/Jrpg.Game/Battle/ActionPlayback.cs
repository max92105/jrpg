using System.Globalization;
using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One turn's events as the beats the screen plays (design key decision 4): a banner for a skill, an item or Defend, an
/// impact when effects land, a defeat when someone falls. The hold lasts as long as the beats; Defend and Run hold
/// nothing. On the impact, each landed effect pops its amount; whoever fell shows white, then fades on the defeat beat.
/// An event it doesn't know throws, so a new mechanic can't be shown silently wrong (ADR 0004).
/// </summary>
internal sealed class ActionPlayback
{
    // Legacy AnimateDeath: the fallen stay lit white for 1.0 s, then fade out over 1.0 s, easing in.
    private static readonly TimeSpan _fallTint = TimeSpan.FromSeconds(1.0);
    private static readonly TimeSpan _fallFade = TimeSpan.FromSeconds(1.0);

    private readonly IReadOnlyList<Combatant> _fallen;

    private ActionPlayback(IReadOnlyList<PlaybackBeat> beats, string? bannerText, IReadOnlyList<FloatingNumber> numbers, IReadOnlyList<Combatant> fallen)
    {
        Beats = beats;
        BannerText = bannerText;
        Numbers = numbers;
        _fallen = fallen;
        Duration = beats.Aggregate(TimeSpan.Zero, (total, beat) => total + beat.Duration);
    }

    public IReadOnlyList<PlaybackBeat> Beats { get; }

    /// <summary>What the banner says for this action; null when it shows nothing.</summary>
    public string? BannerText { get; }

    /// <summary>How long the screen holds for the beats.</summary>
    public TimeSpan Duration { get; }

    /// <summary>One number per landed effect, built once; they all pop at the impact, which comes first.</summary>
    public IReadOnlyList<FloatingNumber> Numbers { get; }

    /// <summary>Time since the action landed; it runs on after the hold, while the numbers and the fallen fade.</summary>
    public TimeSpan Elapsed { get; private set; }

    /// <param name="events">One turn's events, as <see cref="BattleState.Execute"/> returned them.</param>
    /// <param name="messages">How "{name} defends." is written.</param>
    /// <exception cref="InvalidOperationException">An event or a command the battle screen can't show yet.</exception>
    public static ActionPlayback Start(IReadOnlyList<BattleEvent> events, UiMessages messages)
    {
        var beats = new List<PlaybackBeat>();
        var numbers = new List<FloatingNumber>();
        var fallen = new List<Combatant>();
        string? bannerText = null;
        foreach (var battleEvent in events)
        {
            switch (battleEvent)
            {
                case ActionStarted started:
                    bannerText = BannerTextFor(started, messages);
                    if (bannerText is not null)
                    {
                        AddOnce(beats, PlaybackBeatKind.Banner, TimeSpan.Zero);
                    }

                    break;
                case EffectApplied effect:
                    AddOnce(beats, PlaybackBeatKind.Impact, BattlePacing.ImpactHold);
                    numbers.Add(new FloatingNumber(effect.Target, effect.Amount.ToString(CultureInfo.InvariantCulture), effect.Kind));
                    break;
                case CombatantDefeated defeated:
                    AddOnce(beats, PlaybackBeatKind.Defeat, BattlePacing.DefeatHold - BattlePacing.ImpactHold);
                    fallen.Add(defeated.Combatant);
                    break;
                case BattleEnded:
                    break;
                default:
                    throw new InvalidOperationException($"The battle screen can't show a {battleEvent.GetType().Name} yet.");
            }
        }

        return new ActionPlayback(beats, bannerText, numbers, fallen);
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

    // Effects landing together make one impact, and any falls one defeat, however many targets there are.
    private static void AddOnce(List<PlaybackBeat> beats, PlaybackBeatKind kind, TimeSpan duration)
    {
        if (!beats.Exists(beat => beat.Kind == kind))
        {
            beats.Add(new PlaybackBeat(kind, duration));
        }
    }
}
