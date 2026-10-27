using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// An effect as its user lands it: Power raises every raw value a member deals, damage and healing alike, and
/// Intelligence shapes the status it grants (decision 296). A turn readied carries no raw value, so it lands as it is.
/// </summary>
internal static class EffectScaling
{
    /// <returns>
    /// A damage, healing or revive effect with its power put through <see cref="StatFormula.Scaled"/>, and any grant
    /// through <see cref="GrantScaling.Scaled"/>; others as is.
    /// </returns>
    /// <param name="effect">The effect as content wrote it.</param>
    /// <param name="user">The stats of whoever uses it.</param>
    public static Effect ScaledFor(Effect effect, CombatStats user)
    {
        var scaled = effect.Kind switch
        {
            EffectKind.Damage or EffectKind.RestoreHp or EffectKind.Revive => effect with { Power = StatFormula.Scaled(effect.Power, user.DamagePermille) },
            _ => effect,
        };
        return scaled.Status is { } grant
            ? scaled with { Status = GrantScaling.Scaled(grant, user.StatusPermille, user.ExtraStatusTurns) }
            : scaled;
    }
}
