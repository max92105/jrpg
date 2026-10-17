namespace Jrpg.Core.Battle;

/// <summary>
/// Placeholder ported from the demo (plan 2026-10-17, guiding rule): defense subtracts straight from power,
/// with a minimum so every hit still counts.
/// </summary>
internal static class DamageFormula
{
    public const int MinimumDamage = 1;

    public static int Calculate(int power, int defense) => Math.Max(MinimumDamage, power - defense);
}
