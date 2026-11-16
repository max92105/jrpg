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

    /// <summary>
    /// The most a tiered amount reaches, so <see cref="Calculate"/>'s product (up to about 1.8 x 10^12 times the power)
    /// stays within a <see cref="long"/> (decision 818).
    /// </summary>
    public const int MaxPower = 1_000_000;

    /// <summary>The crit factor of a hit that didn't crit: the power as it is.</summary>
    public const int NormalHit = 1000;

    /// <summary>
    /// The least resistance a hit is cued "Resisted" at: a small one, from Constitution or Will alone, would cue nearly
    /// every hit, so only one that takes a quarter off is worth saying (decision 323).
    /// </summary>
    public const int ResistedThresholdPercent = 25;

    /// <summary>
    /// What a blocked hit deals of what it would (decision 348, placeholder): its share joins the one product below
    /// rather than rounding on its own, so a blocked crit loses no point to a step of its own.
    /// </summary>
    public const int BlockedDamagePercent = 50;

    private const long Whole = 1000;

    private const long WholePercent = 100;

    /// <returns>
    /// <c>max(1, power × crit × (1000 − resistance) × (1000 + extra taken) × blocked share ÷ (1000³ ×
    /// 100))</c>, in a <c>long</c> and rounded down once, so a crit then a resistance never loses a point to a step of
    /// its own. The resistance is held within
    /// <see cref="Resistances.FloorPercent"/>..<see cref="Resistances.CapPercent"/>; the blocked share is 100 %, or
    /// <see cref="BlockedDamagePercent"/> for a blocked hit. The product stays far within a <c>long</c>: a power of
    /// 10,000 at a 300 % crit against a −100 % resistance with 200 % taken comes to 1.8 × 10^16.
    /// </returns>
    /// <param name="power">The hit's power, already raised by its user's Power.</param>
    /// <param name="critPermille">The hit's crit damage, or <see cref="NormalHit"/>.</param>
    /// <param name="mitigation">
    /// The target's resistance to the hit, the extra share its statuses add, and whether it blocked.
    /// </param>
    public static int Calculate(int power, int critPermille, Mitigation mitigation)
    {
        var resistance = Math.Clamp(
            mitigation.ResistancePermille,
            StatFormula.PermilleOf(Resistances.FloorPercent),
            StatFormula.PermilleOf(Resistances.CapPercent));
        var blockedShare = mitigation.IsBlocked ? BlockedDamagePercent : WholePercent;
        var damage = (long)power * critPermille * (Whole - resistance) * (Whole + mitigation.ExtraTakenPermille) * blockedShare
            / (Whole * Whole * Whole * WholePercent);
        return (int)Math.Max(MinimumDamage, damage);
    }

    /// <summary>
    /// A tiered effect's amount (ADR 0056): Power times the record's <paramref name="perPower"/> times 1 plus the sum
    /// of the damage modifiers, rounded down once and held within 0 and <see cref="MaxPower"/>. The developer's Damage
    /// Test sheet (decision 704): at Power 1, a Low tier (2) with modifiers of 33 % and 440 % gives 11.46, kept as 11.
    /// </summary>
    /// <param name="power">The user's Power.</param>
    /// <param name="perPower">The record's amount per point of Power.</param>
    /// <param name="modifierPermille">The modifiers' sum in permille; -1000 cancels the amount.</param>
    public static int TieredAmount(int power, int perPower, int modifierPermille)
    {
        var amount = (long)power * perPower * (Whole + modifierPermille) / Whole;
        return (int)Math.Clamp(amount, 0, MaxPower);
    }

    /// <summary>
    /// Any flat damage's amount, such as the basic attack's: its raw damage times Power's percent times 1 plus the sum
    /// of the damage modifiers (decisions 784, 786), in one <c>long</c> product rounded down once and never below 0. A
    /// weapon of 26 at 115 % while Weakened (-25 %) deals 22.425, kept as 22; rounding after either factor gives 21.
    /// </summary>
    /// <param name="raw">The raw damage: an untiered skill's power, a weapon's, the unarmed one, or an enemy's.</param>
    /// <param name="damagePermille">The user's damage percent from Power, in permille.</param>
    /// <param name="modifierPermille">The modifiers' sum in permille; -1000 cancels the damage.</param>
    public static int ModifiedFlat(int raw, int damagePermille, int modifierPermille)
    {
        var amount = (long)raw * damagePermille * (Whole + modifierPermille) / (Whole * Whole);
        return (int)Math.Max(0, amount);
    }

    /// <returns>
    /// <c>hpTaken × permille ÷ 1000</c>, rounded down: a share of what the hit really took, so overkill steals
    /// nothing, and per hit, so a small hit can steal 0 (decision 365).
    /// </returns>
    /// <param name="hpTaken">The HP the hit took from its target.</param>
    /// <param name="permille">The hitter's lifesteal.</param>
    public static int LifeStolen(int hpTaken, int permille) => (int)((long)hpTaken * permille / Whole);

    /// <returns>
    /// A positive resistance held at the cap, then less the penetration, never below 0; a resistance at or below 0 as
    /// it is. Points rather than a share, after the cap (decision 364, the ARPG convention), so a few points show
    /// against any armor; a weakness isn't deepened, since penetration answers armor and isn't extra damage.
    /// </returns>
    /// <param name="resistancePermille">The target's resistance to the hit's type.</param>
    /// <param name="penetrationPermille">The hitter's armor penetration.</param>
    public static int PenetratedResistance(int resistancePermille, int penetrationPermille)
    {
        if (resistancePermille <= 0)
        {
            return resistancePermille;
        }

        var capped = Math.Min(resistancePermille, StatFormula.PermilleOf(Resistances.CapPercent));
        return Math.Max(0, capped - penetrationPermille);
    }

    /// <summary>Whether a hit meeting <paramref name="resistancePermille"/> is cued weak, resisted, or not.</summary>
    public static HitAffinity AffinityOf(int resistancePermille) => resistancePermille switch
    {
        < 0 => HitAffinity.Weak,
        _ when resistancePermille >= StatFormula.PermilleOf(ResistedThresholdPercent) => HitAffinity.Resisted,
        _ => HitAffinity.Neutral,
    };
}
