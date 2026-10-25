using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>Applies an effect's power to one target. Targeting is checked before this is called.</summary>
internal static class EffectResolver
{
    /// <returns>The amount actually applied: HP lost, restored or revived with; 0 for a status-only effect.</returns>
    /// <param name="effect">What lands.</param>
    /// <param name="target">Whose HP it changes.</param>
    /// <param name="targetDefense">The target's defense now.</param>
    /// <param name="damageTakenPercent">How much of a hit lands, from the target's statuses (question 215).</param>
    public static int Apply(Effect effect, Vitals target, int targetDefense, int damageTakenPercent = DamageFormula.NoChangePercent) => effect.Kind switch
    {
        // Placeholder: defense reduces skill and item damage like an attack, as in the demo (plan 2026-10-17, guiding rule).
        EffectKind.Damage => target.TakeDamage(DamageFormula.Calculate(effect.Power, targetDefense, damageTakenPercent)),
        EffectKind.RestoreHp => target.RestoreHp(effect.Power),
        EffectKind.Revive => target.Revive(effect.Power),
        EffectKind.GrantStatus => 0,
        EffectKind.ReadyExtraTurn => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "Unknown effect kind."),
    };
}
