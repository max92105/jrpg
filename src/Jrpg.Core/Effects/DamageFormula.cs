using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>
/// A hit's damage (ADR 0027): its power, already raised by its user's Power, multiplied by its crit damage when it
/// crits (decision 300), then scaled by the target's resistance to its type and by its statuses, such as Exposed's +50
/// % (question 215). Shares multiply rather than subtract, so a hit keeps its worth against any target, and a minimum
/// keeps every hit counting.
/// </summary>
internal static class DamageFormula
{
    public const int MinimumDamage = 1;

    /// <summary>The crit factor of a hit that didn't crit: the power as it is.</summary>
    public const int NormalHit = 1000;

    /// <summary>
    /// The least resistance a hit is cued "Resisted" at: a small one, from Constitution or Will alone, would cue nearly
    /// every hit, so only one that takes a quarter off is worth saying (decision 323).
    /// </summary>
    public const int ResistedThresholdPercent = 25;

    private const long Whole = 1000;

    /// <returns>
    /// <c>max(1, power × crit × (1000 − resistance) × (1000 + extra taken) ÷ 1000³)</c>, in a <c>long</c> and
    /// rounded down once, so a crit then a resistance never loses a point to a step of its own. The resistance is held
    /// within <see cref="Resistances.FloorPercent"/>..<see cref="Resistances.CapPercent"/>.
    /// </returns>
    /// <param name="power">The hit's power, already raised by its user's Power.</param>
    /// <param name="critPermille">The hit's crit damage, or <see cref="NormalHit"/>.</param>
    /// <param name="mitigation">The target's resistance to the hit, and the extra share its statuses add.</param>
    public static int Calculate(int power, int critPermille, Mitigation mitigation)
    {
        var resistance = Math.Clamp(
            mitigation.ResistancePermille,
            StatFormula.PermilleOf(Resistances.FloorPercent),
            StatFormula.PermilleOf(Resistances.CapPercent));
        var damage = (long)power * critPermille * (Whole - resistance) * (Whole + mitigation.ExtraTakenPermille) / (Whole * Whole * Whole);
        return (int)Math.Max(MinimumDamage, damage);
    }

    /// <returns>
    /// <c>hpTaken × permille ÷ 1000</c>, rounded down: a share of what the hit really took, so overkill steals
    /// nothing, and per hit, so a small hit can steal 0 (decision 365).
    /// </returns>
    /// <param name="hpTaken">The HP the hit took from its target.</param>
    /// <param name="permille">The hitter's lifesteal.</param>
    public static int LifeStolen(int hpTaken, int permille) => (int)((long)hpTaken * permille / Whole);

    /// <summary>Whether a hit meeting <paramref name="resistancePermille"/> is cued weak, resisted, or not.</summary>
    public static HitAffinity AffinityOf(int resistancePermille) => resistancePermille switch
    {
        < 0 => HitAffinity.Weak,
        _ when resistancePermille >= StatFormula.PermilleOf(ResistedThresholdPercent) => HitAffinity.Resisted,
        _ => HitAffinity.Neutral,
    };
}
