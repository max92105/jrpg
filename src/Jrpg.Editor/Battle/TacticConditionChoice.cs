using Jrpg.Core.Battle;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One trigger as a rule's When box offers it, with its words (design.md, section 3.1); a null kind is always.
/// </summary>
internal sealed record TacticConditionChoice(TacticConditionKind? Kind, string Label)
{
    /// <summary>Always, then every kind in the enum's order, whatever the rule's skill (decision 850).</summary>
    public static IReadOnlyList<TacticConditionChoice> All { get; } =
    [
        new(null, "always"),
        new(TacticConditionKind.OwnHpBelow, "its HP is below"),
        new(TacticConditionKind.AllyHpBelow, "an ally's HP is below"),
        new(TacticConditionKind.AllyFallen, "an ally has fallen"),
        new(TacticConditionKind.WasHit, "it was hit"),
        new(TacticConditionKind.WasCriticallyHit, "it was critically hit"),
        new(TacticConditionKind.WasHitOnWeakness, "it was hit on a weakness"),
    ];

    /// <exception cref="InvalidOperationException">A kind given no words yet, which the guard test catches.</exception>
    public static TacticConditionChoice Of(TacticConditionKind? kind) => All.Single(choice => choice.Kind == kind);
}
