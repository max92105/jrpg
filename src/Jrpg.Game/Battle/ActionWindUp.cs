using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// What one action shows before it lands (ADR 0013 amended, decision 644): its time from the command to the impact, the
/// plaque's words (decision 646), its opening sound (decision 647), its actor's blink (decision 648), the impact
/// strip's lead on the targets a damaging action aims at (decision 656), and the swing of an Attack's or an action's
/// own strip (decisions 657, 708) with its whoosh as it starts (decision 658), shown from the command alone, since Core
/// executes it only when the wind-up ends.
/// </summary>
internal sealed class ActionWindUp : ISoundTimeline
{
    private ActionWindUp(
        Combatant actor, BattleCommand command, UiMessages messages, IReadOnlyDictionary<string, EffectStripDefinition> strips)
    {
        Actor = actor;
        Duration = DurationOf(command);
        BannerText = ActionBannerText.For(actor, command, messages);
        AimedTargets = AimedTargetsOf(command);
        ImpactFile = AimedTargets.Count > 0 ? ActionStrip.FileOf(actor, command) : null;
        IsMirrored = EffectOnTarget.IsMirroredFor(actor);
        SwingFile = ImpactFile is null ? null : ActionStrip.SwingFileOf(actor, command, strips);
        Sounds = SoundsOf(command, SwingFile);
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
    /// The action's opening sound at its start, with the plaque (decision 647), then the swing's whoosh as the swing
    /// starts (decision 658), in time order; empty when it has neither.
    /// </summary>
    public IReadOnlyList<TimedSound> Sounds { get; }

    /// <summary>The time since the action started.</summary>
    public TimeSpan Elapsed { get; private set; }

    /// <summary>
    /// Who a damaging action aims at, its impact strip's lead playing on each (decision 656): an Attack's target, a
    /// damaging skill's or item's targets (Q2); none for an action that doesn't damage.
    /// </summary>
    public IReadOnlyList<Combatant> AimedTargets { get; }

    /// <summary>The action's strip while it damages, its own or its actor's (decision 678); null otherwise.</summary>
    public string? ImpactFile { get; }

    /// <summary>
    /// Whether the impact strip is drawn mirrored: a party member's, as effect art faces right (decision 655).
    /// </summary>
    public bool IsMirrored { get; }

    /// <summary>The time since the impact: negative while the action winds up, zero at the impact.</summary>
    public TimeSpan SinceImpact => Elapsed - Duration;

    /// <summary>
    /// The swing played in front of the actor, named by the record of an Attack's or an action's own strip (decisions
    /// 657, 708); null for a skill or item on its actor's strip, an action that doesn't damage, or a strip that names
    /// no swing of its own.
    /// </summary>
    public string? SwingFile { get; }

    /// <summary>The time since the swing started, <see cref="BattlePacing.SwingStart"/> into the wind-up.</summary>
    public TimeSpan SinceSwing => Elapsed - BattlePacing.SwingStart;

    /// <exception cref="InvalidOperationException">The command is of a kind the screen can't show yet.</exception>
    /// <param name="actor">Who acts.</param>
    /// <param name="command">What they do, as chosen.</param>
    /// <param name="messages">How the plaque writes what the actor does.</param>
    /// <param name="strips">
    /// The effect strips' records (ADR 0048), which name the swing of an Attack's or an action's own strip.
    /// </param>
    public static ActionWindUp Of(
        Combatant actor, BattleCommand command, UiMessages messages, IReadOnlyDictionary<string, EffectStripDefinition> strips) =>
        new(actor, command, messages, strips);

    // An Attack always damages; a skill or an item when its effect is damage (decision 657's Q2).
    private static IReadOnlyList<Combatant> AimedTargetsOf(BattleCommand command) => command switch
    {
        AttackCommand attack => [attack.Target],
        SkillCommand { Skill.Effect.Kind: EffectKind.Damage } skill => skill.Targets,
        ItemCommand { Item.Effect.Kind: EffectKind.Damage } item => item.Targets,
        _ => [],
    };

    // The opening sound at 0 comes before the swing's at BattlePacing.SwingStart, so the list is in time order.
    private static List<TimedSound> SoundsOf(BattleCommand command, string? swingFile)
    {
        List<TimedSound> sounds = ActionSounds.OpeningOf(command) is { } file ? [new TimedSound(file, TimeSpan.Zero)] : [];
        if (swingFile is not null)
        {
            sounds.Add(new TimedSound(UiSoundFiles.Battle.Swing, BattlePacing.SwingStart));
        }

        return sounds;
    }

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
