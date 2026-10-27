using Jrpg.Core.Effects;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Battle;

/// <summary>
/// An effect as its user lands it: Power raises every raw value a member deals, damage and healing alike (decision
/// 296). A status grant and a turn readied carry no raw value, so they land as they are.
/// </summary>
internal static class EffectScaling
{
    /// <returns>
    /// A damage, healing or revive effect with its power put through <see cref="StatFormula.Scaled"/>; others as is.
    /// </returns>
    public static Effect ScaledFor(Effect effect, int damagePermille) => effect.Kind switch
    {
        EffectKind.Damage or EffectKind.RestoreHp or EffectKind.Revive => effect with { Power = StatFormula.Scaled(effect.Power, damagePermille) },
        _ => effect,
    };
}
