using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// An effect as its user lands it: a tiered effect's amount is its user's Power times its record's per-Power (ADR
/// 0056), and Power's percent raises a flat value only, damage and healing alike (decision 784); Intelligence shapes
/// the status it grants (decision 296). A turn readied carries no raw value, so it lands as it is.
/// </summary>
internal static class EffectScaling
{
    /// <returns>
    /// A damage, healing or revive effect with its power from its tier, else put through
    /// <see cref="StatFormula.Scaled"/>, and every grant through <see cref="GrantScaling.Scaled"/>; others as is.
    /// </returns>
    /// <param name="effect">The effect as content wrote it.</param>
    /// <param name="user">The stats of whoever uses it.</param>
    public static Effect ScaledFor(Effect effect, CombatStats user)
    {
        var scaled = effect.Kind switch
        {
            EffectKind.Damage or EffectKind.RestoreHp or EffectKind.Revive => effect with { Power = AmountOf(effect, user) },
            _ => effect,
        };
        return scaled with
        {
            Statuses = [.. scaled.Statuses.Select(
                grant => GrantScaling.Scaled(grant, user.StatusPermille, user.ExtraStatusTurns))],
        };
    }

    // The damage modifiers' sum is 0 until a status gives one (Weakened, decision 786).
    private static int AmountOf(Effect effect, CombatStats user) => effect.Tier is { } tier
        ? DamageFormula.TieredAmount(user.Power, tier.PerPower, modifierPermille: 0)
        : StatFormula.Scaled(effect.Power, user.DamagePermille);
}
