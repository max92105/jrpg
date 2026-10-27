using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>Applies an effect's power to one target. Targeting is checked before this is called.</summary>
internal static class EffectResolver
{
    /// <returns>The amount actually applied: HP lost, restored or revived with; 0 for a status-only effect.</returns>
    /// <param name="effect">What lands.</param>
    /// <param name="target">Whose HP it changes.</param>
    /// <param name="mitigation">What stands between a hit and the target's HP now.</param>
    public static int Apply(Effect effect, Vitals target, Mitigation mitigation) => effect.Kind switch
    {
        EffectKind.Damage => target.TakeDamage(DamageFormula.Calculate(effect.Power, mitigation)),
        EffectKind.RestoreHp => target.RestoreHp(effect.Power),
        EffectKind.Revive => target.Revive(effect.Power),
        EffectKind.GrantStatus => 0,
        EffectKind.ReadyExtraTurn => 0,
        // A rest has no single target: FieldItemUse rests every member, and battle refuses a rest item.
        EffectKind.Rest => throw new InvalidOperationException("A rest effect never lands on one target."),
        _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "Unknown effect kind."),
    };
}
