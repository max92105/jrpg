using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One turn's events as the beats the screen plays (design key decision 4): a banner for a skill, an item or Defend, an
/// impact when effects land, a defeat when someone falls. The hold lasts as long as the beats; Defend and Run hold
/// nothing. An event it doesn't know throws, so a new mechanic can't be shown silently wrong (ADR 0004).
/// </summary>
internal sealed class ActionPlayback
{
    private ActionPlayback(IReadOnlyList<PlaybackBeat> beats, string? bannerText)
    {
        Beats = beats;
        BannerText = bannerText;
        Duration = beats.Aggregate(TimeSpan.Zero, (total, beat) => total + beat.Duration);
    }

    public IReadOnlyList<PlaybackBeat> Beats { get; }

    /// <summary>What the banner says for this action; null when it shows nothing.</summary>
    public string? BannerText { get; }

    /// <summary>How long the screen holds for the beats.</summary>
    public TimeSpan Duration { get; }

    /// <param name="events">One turn's events, as <see cref="BattleState.Execute"/> returned them.</param>
    /// <param name="messages">How "{name} defends." is written.</param>
    /// <exception cref="InvalidOperationException">An event or a command the battle screen can't show yet.</exception>
    public static ActionPlayback Start(IReadOnlyList<BattleEvent> events, UiMessages messages)
    {
        var beats = new List<PlaybackBeat>();
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
                case EffectApplied:
                    AddOnce(beats, PlaybackBeatKind.Impact, BattlePacing.ImpactHold);
                    break;
                case CombatantDefeated:
                    AddOnce(beats, PlaybackBeatKind.Defeat, BattlePacing.DefeatHold - BattlePacing.ImpactHold);
                    break;
                case BattleEnded:
                    break;
                default:
                    throw new InvalidOperationException($"The battle screen can't show a {battleEvent.GetType().Name} yet.");
            }
        }

        return new ActionPlayback(beats, bannerText);
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
