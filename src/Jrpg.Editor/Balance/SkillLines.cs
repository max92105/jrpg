using System.Globalization;
using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Balance;

/// <summary>
/// A skill's texts in the balance table, whoever uses it: what its amount comes from, and what it comes to for a user
/// with the table's damage modifier, worked out by Core's own rule (decision 993).
/// </summary>
internal static class SkillLines
{
    /// <returns>
    /// "fire_1 x2", the tier and its per-Power, or "flat 15", the raw power, for an effect naming no tier.
    /// </returns>
    public static string TierLine(Effect effect) =>
        effect.Tier is { } tier
            ? $"{tier.Id} x{TypedNumber.Text(tier.PerPower)}"
            : $"flat {TypedNumber.Text(effect.Power)}";

    /// <returns>"24" for one hit, "28 x 4 = 112" for several, the total in a long.</returns>
    /// <param name="effect">A damage, healing or revive effect (<see cref="EffectAmounts.ChangesHp"/>).</param>
    /// <param name="user">The user's stats as the battle builds them.</param>
    /// <param name="modifierPermille">
    /// The table's damage modifier, added to the user's modifiers' sum, so Core decides what it raises: damage, never
    /// healing (decision 786).
    /// </param>
    public static string AmountText(Effect effect, CombatStats user, int modifierPermille)
    {
        var modified = user with { DamageModifierPermille = user.DamageModifierPermille + modifierPermille };
        var perHit = EffectAmounts.PerHit(effect, modified);
        var total = ((long)perHit * effect.Hits).ToString(CultureInfo.InvariantCulture);
        return effect.Hits > 1
            ? $"{TypedNumber.Text(perHit)} x {TypedNumber.Text(effect.Hits)} = {total}"
            : TypedNumber.Text(perHit);
    }
}
