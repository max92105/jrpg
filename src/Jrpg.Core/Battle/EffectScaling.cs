using Jrpg.Core.Effects;

namespace Jrpg.Core.Battle;

/// <summary>
/// An effect as its user lands it: Power raises every raw value a member deals, damage and healing alike (decision
/// 296). A status grant and a turn readied carry no raw value, so they land as they are.
/// </summary>
internal static class EffectScaling
{
    private const long Whole = 1000;

    /// <returns>
    /// The effect with a damage, healing or revive power of <c>power × damagePermille ÷ 1000</c>, in a <c>long</c> and
    /// rounded down once (ADR 0027); any other effect unchanged.
    /// </returns>
    public static Effect ScaledFor(Effect effect, int damagePermille) => effect.Kind switch
    {
        EffectKind.Damage or EffectKind.RestoreHp or EffectKind.Revive => effect with { Power = (int)((long)effect.Power * damagePermille / Whole) },
        _ => effect,
    };
}
