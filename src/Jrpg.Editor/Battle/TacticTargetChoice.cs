using Jrpg.Core.Battle;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One target rule as a rule's On box offers it, with its words (design.md, section 3.2); a null target is automatic.
/// </summary>
internal sealed record TacticTargetChoice(TacticTarget? Target, string Label)
{
    /// <summary>Automatic, then every target in the enum's order, whatever the rule's skill (decision 850).</summary>
    public static IReadOnlyList<TacticTargetChoice> All { get; } =
    [
        new(null, "automatic"),
        new(TacticTarget.Subject, "that one"),
        new(TacticTarget.Self, "itself"),
        new(TacticTarget.LowestHpFoe, "the foe with the lowest HP"),
        new(TacticTarget.HighestHpFoe, "the foe with the highest HP"),
        new(TacticTarget.LowestHpOnItsSide, "the lowest HP on its side (itself included)"),
        new(TacticTarget.UnafflictedFoe, "a foe without its statuses"),
    ];

    /// <exception cref="InvalidOperationException">
    /// A target given no words yet, which the guard test catches.
    /// </exception>
    public static TacticTargetChoice Of(TacticTarget? target) => All.Single(choice => choice.Target == target);
}
