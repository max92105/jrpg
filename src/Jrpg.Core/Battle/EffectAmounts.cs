using Jrpg.Core.Effects;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Battle;

/// <summary>
/// What an effect deals or heals in one hit before its target (decision 993): the editor's balance table reads it, so
/// its amounts are <see cref="EffectScaling"/>'s, the user's damage modifiers raising damage and never healing.
/// </summary>
public static class EffectAmounts
{
    /// <returns>
    /// The power a damage, healing or revive effect lands with from <paramref name="user"/>, before the target's
    /// resistance, crits and blocks.
    /// </returns>
    /// <exception cref="ArgumentException">The effect's kind changes no HP.</exception>
    public static int PerHit(Effect effect, CombatStats user) =>
        effect.Kind is EffectKind.Damage or EffectKind.RestoreHp or EffectKind.Revive
            ? EffectScaling.ScaledFor(effect, user).Power
            : throw new ArgumentException($"A {effect.Kind} effect changes no HP.", nameof(effect));
}
