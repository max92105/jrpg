using System.Globalization;

namespace Jrpg.Game.Ui;

/// <summary>
/// Whole numbers as the game shows them, the same on every machine: the invariant culture writes no thousands separator
/// and an ASCII minus, which every font of the game has.
/// </summary>
internal static class NumberText
{
    public static string Of(int value) => value.ToString(CultureInfo.InvariantCulture);
}
