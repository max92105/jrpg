using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>Applies an effect's power to one target. Targeting is checked before this is called.</summary>
internal static class EffectResolver
{
    /// <returns>The amount actually applied: HP lost, HP restored, or HP revived with.</returns>
    public static int Apply(Effect effect, Vitals target, int targetDefense) => effect.Kind switch
    {
        // Placeholder: defense reduces skill and item damage like an attack, as in the demo (plan 2026-10-17, guiding rule).
        EffectKind.Damage => target.TakeDamage(DamageFormula.Calculate(effect.Power, targetDefense)),
        EffectKind.RestoreHp => target.RestoreHp(effect.Power),
        EffectKind.Revive => target.Revive(effect.Power),
        _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "Unknown effect kind."),
    };
}
