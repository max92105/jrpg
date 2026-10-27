using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>
/// A hit's damage (ADR 0027): its power, already raised by its user's Power, scaled by the target's resistance to its
/// type and by its statuses, such as Exposed's +50 % (question 215). Shares multiply rather than subtract, so a hit
/// keeps its worth against any target, and a minimum keeps every hit counting.
/// </summary>
internal static class DamageFormula
{
    public const int MinimumDamage = 1;

    /// <summary>
    /// The least resistance a hit is cued "Resisted" at: a small one, from Constitution or Will alone, would cue nearly
    /// every hit, so only one that takes a quarter off is worth saying (decision 323).
    /// </summary>
    public const int ResistedThresholdPercent = 25;

    private const long Whole = 1000;

    /// <returns>
    /// <c>max(1, power × (1000 − resistance) × (1000 + extra taken) ÷ 1000²)</c>, in a <c>long</c> and rounded
    /// down once. The resistance is held within <see cref="Resistances.FloorPercent"/>..
    /// <see cref="Resistances.CapPercent"/>.
    /// </returns>
    public static int Calculate(int power, Mitigation mitigation)
    {
        var resistance = Math.Clamp(
            mitigation.ResistancePermille,
            StatFormula.PermilleOf(Resistances.FloorPercent),
            StatFormula.PermilleOf(Resistances.CapPercent));
        var damage = power * (Whole - resistance) * (Whole + mitigation.ExtraTakenPermille) / (Whole * Whole);
        return (int)Math.Max(MinimumDamage, damage);
    }

    /// <summary>Whether a hit meeting <paramref name="resistancePermille"/> is cued weak, resisted, or not.</summary>
    public static HitAffinity AffinityOf(int resistancePermille) => resistancePermille switch
    {
        < 0 => HitAffinity.Weak,
        _ when resistancePermille >= StatFormula.PermilleOf(ResistedThresholdPercent) => HitAffinity.Resisted,
        _ => HitAffinity.Neutral,
    };
}
