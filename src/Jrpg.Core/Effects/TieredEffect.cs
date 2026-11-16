using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>
/// A named damage or heal whose amount is its user's Power times its <paramref name="PerPower"/> (ADR 0056, decision
/// 782): <c>fire_1</c>, <c>heal_2</c>, the suffix naming the developer's Damage Test tier.
/// </summary>
/// <param name="Id">The record's id, such as <c>fire_1</c>.</param>
/// <param name="Kind">Damage or a heal.</param>
/// <param name="PerPower">What each point of its user's Power gives, from 1.</param>
public sealed record TieredEffect(string Id, EffectKind Kind, int PerPower)
{
    /// <summary>A damage record's type, which the target's resistance meets; null for a heal.</summary>
    public DamageType? DamageType { get; init; }

    /// <summary>
    /// One hit's amount at that Power before any modifier and target (decision 1001): Power times
    /// <see cref="PerPower"/>, through <see cref="DamageFormula.TieredAmount"/>, so held within 0 and
    /// <see cref="DamageFormula.MaxPower"/>. The Effects page's "At Power 10" line reads it.
    /// </summary>
    public int AmountAt(int power) => DamageFormula.TieredAmount(power, PerPower, 0);
}
