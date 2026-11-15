using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// What one action shows before it lands (ADR 0013 amended, decision 644): its time from the command to the impact, the
/// plaque's words (decision 646), its opening sound (decision 647) and its actor's blink (decision 648), shown from the
/// command alone, since Core executes it only when the wind-up ends.
/// </summary>
internal sealed class ActionWindUp : ISoundTimeline
{
    private ActionWindUp(Combatant actor, TimeSpan duration, string? bannerText, IReadOnlyList<TimedSound> sounds)
    {
        Actor = actor;
        Duration = duration;
        BannerText = bannerText;
        Sounds = sounds;
    }

    public Combatant Actor { get; }

    /// <summary>
    /// <see cref="BattlePacing.WindUp"/> for an Attack, a skill or an item; nothing for Defend, Move or Run, which land
    /// at once.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>What the plaque says from the wind-up's start (decision 646); null when it shows nothing.</summary>
    public string? BannerText { get; }

    /// <summary>
    /// The action's opening sound at its start, with the plaque (decision 647); empty when it has none.
    /// </summary>
    public IReadOnlyList<TimedSound> Sounds { get; }

    /// <summary>The time since the action started.</summary>
    public TimeSpan Elapsed { get; private set; }

    /// <exception cref="InvalidOperationException">The command is of a kind the screen can't show yet.</exception>
    /// <param name="actor">Who acts.</param>
    /// <param name="command">What they do, as chosen.</param>
    /// <param name="messages">How the plaque writes what the actor does.</param>
    public static ActionWindUp Of(Combatant actor, BattleCommand command, UiMessages messages) =>
        new(actor, DurationOf(command), ActionBannerText.For(actor, command, messages), OpeningOf(command));

    private static IReadOnlyList<TimedSound> OpeningOf(BattleCommand command) =>
        ActionSounds.OpeningOf(command) is { } file ? [new TimedSound(file, TimeSpan.Zero)] : [];

    private static TimeSpan DurationOf(BattleCommand command) => command switch
    {
        AttackCommand or SkillCommand or ItemCommand => BattlePacing.WindUp,
        DefendCommand or MoveCommand or FleeCommand => TimeSpan.Zero,
        _ => throw new InvalidOperationException($"The battle screen can't wind up a {command.GetType().Name} yet."),
    };

    public void Update(TimeSpan delta) => Elapsed += delta;

    /// <summary>
    /// How strongly <paramref name="combatant"/> blinks now (decision 648): the actor only, and only while the action
    /// winds up, since the sequence keeps the last wind-up's clock running until the next action.
    /// </summary>
    /// <returns>From 0 to 1, as <see cref="ActorFlash.StrengthAt"/>.</returns>
    public float BlinkOf(Combatant combatant) =>
        combatant == Actor && Elapsed < Duration ? ActorFlash.StrengthAt(Elapsed) : 0;
}
