using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// What one action shows before it lands (ADR 0013 amended, decision 644): its time from the command to the impact, the
/// plaque's words (decision 646), its opening sound (decision 647), its actor's blink (decision 648) and the impact
/// strip's lead on the targets a damaging action aims at (decision 656) and an Attack's swing (decision 657), shown
/// from the command alone, since Core executes it only when the wind-up ends.
/// </summary>
internal sealed class ActionWindUp : ISoundTimeline
{
    private ActionWindUp(
        Combatant actor, BattleCommand command, UiMessages messages, IReadOnlyDictionary<string, EffectStripDefinition> strips)
    {
        Actor = actor;
        Duration = DurationOf(command);
        BannerText = ActionBannerText.For(actor, command, messages);
        Sounds = OpeningOf(command);
        AimedTargets = AimedTargetsOf(command);
        ImpactFile = AimedTargets.Count > 0 ? BattlerArt.Of(actor).AttackEffectFile : null;
        IsMirrored = EffectOnTarget.IsMirroredFor(actor);
        SwingFile = command is AttackCommand && ImpactFile is { } impact
            ? EffectStripDefinition.Of(impact, strips).SwingFile
            : null;
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

    /// <summary>
    /// Who a damaging action aims at, its impact strip's lead playing on each (decision 656): an Attack's target, a
    /// damaging skill's or item's targets (Q2); none for an action that doesn't damage.
    /// </summary>
    public IReadOnlyList<Combatant> AimedTargets { get; }

    /// <summary>The actor's impact strip while it damages; null otherwise.</summary>
    public string? ImpactFile { get; }

    /// <summary>
    /// Whether the impact strip is drawn mirrored: a party member's, as effect art faces right (decision 655).
    /// </summary>
    public bool IsMirrored { get; }

    /// <summary>The time since the impact: negative while the action winds up, zero at the impact.</summary>
    public TimeSpan SinceImpact => Elapsed - Duration;

    /// <summary>
    /// The swing an Attack plays in front of its attacker, named by the attacker's impact strip's record (decision
    /// 657); null for a skill, an item, an action that doesn't damage, or a strip that names none.
    /// </summary>
    public string? SwingFile { get; }

    /// <summary>The time since the swing started, <see cref="BattlePacing.SwingStart"/> into the wind-up.</summary>
    public TimeSpan SinceSwing => Elapsed - BattlePacing.SwingStart;

    /// <exception cref="InvalidOperationException">The command is of a kind the screen can't show yet.</exception>
    /// <param name="actor">Who acts.</param>
    /// <param name="command">What they do, as chosen.</param>
    /// <param name="messages">How the plaque writes what the actor does.</param>
    /// <param name="strips">The effect strips' records (ADR 0048), which name the attacker's swing.</param>
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
