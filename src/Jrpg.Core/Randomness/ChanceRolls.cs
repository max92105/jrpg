namespace Jrpg.Core.Randomness;

/// <summary>
/// Chance rolls over one random source: the battle's hits and crits, and the loot after it (ADR 0030). A certain
/// outcome draws nothing (ADR 0026), so a fixture whose every hit lands keeps the seeded sequence it had before hits
/// could miss.
/// </summary>
internal sealed class ChanceRolls(IRandom random)
{
    private const int Whole = 1000;

    /// <param name="permille">The chance of success, in tenths of a percent.</param>
    public bool Succeeds(int permille)
    {
        if (permille <= 0)
        {
            return false;
        }

        return permille >= Whole || random.NextBelow(Whole) < permille;
    }
}
