using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>Applies an effect's power to one target. Targeting is checked before this is called.</summary>
internal static class EffectResolver
{
    /// <returns>The amount actually applied: HP lost, restored or revived with; 0 for a status-only effect.</returns>
    public static int Apply(Effect effect, Vitals target, int targetDefense) => effect.Kind switch
    {
        // Placeholder: defense reduces skill and item damage like an attack, as in the demo (plan 2026-10-17, guiding rule).
        EffectKind.Damage => target.TakeDamage(DamageFormula.Calculate(effect.Power, targetDefense)),
        EffectKind.RestoreHp => target.RestoreHp(effect.Power),
        EffectKind.Revive => target.Revive(effect.Power),
        EffectKind.GrantStatus => 0,
        EffectKind.ReadyExtraTurn => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "Unknown effect kind."),
    };
}
