using Jrpg.Core.Randomness;

namespace Jrpg.Core.Battle;

/// <summary>
/// The battle's percent rolls over its one random source. A certain outcome draws nothing (ADR 0026), so a fixture
/// whose every hit lands keeps the seeded sequence it had before hits could miss.
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
