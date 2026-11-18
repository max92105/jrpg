namespace Jrpg.Editor.Balance;

/// <summary>One class in the balance table, with its max HP and Power at each of the table's levels.</summary>
/// <param name="ClassName">The class's name.</param>
/// <param name="HpAndPowerTexts">One per level in the boxes' order: "216 / 39", max HP then Power.</param>
internal sealed record ClassBalanceRow(string ClassName, IReadOnlyList<string> HpAndPowerTexts);
