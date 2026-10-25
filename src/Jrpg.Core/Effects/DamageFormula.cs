namespace Jrpg.Core.Effects;

/// <summary>
/// Placeholder ported from the demo (plan 2026-10-17, guiding rule): defense subtracts straight from power, with a
/// minimum so every hit still counts. A target's statuses then scale what lands, such as Exposed's +50 %
/// (question 215).
/// </summary>
internal static class DamageFormula
{
    public const int MinimumDamage = 1;

    /// <summary>The share of damage a target takes with nothing scaling it.</summary>
    public const int NoChangePercent = 100;

    /// <param name="power">The hit's power.</param>
    /// <param name="defense">The target's defense now.</param>
    /// <param name="damageTakenPercent">How much of the damage lands, 100 for all of it.</param>
    /// <returns>
    /// <c>max(1, (power − defense) × percent ÷ 100)</c>, rounded down. The percent comes after defense, so a setup
    /// like Exposed multiplies what actually gets through rather than the raw power.
    /// </returns>
    public static int Calculate(int power, int defense, int damageTakenPercent = NoChangePercent) =>
        Math.Max(MinimumDamage, (power - defense) * damageTakenPercent / 100);
}
