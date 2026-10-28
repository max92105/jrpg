using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>One line of the victory screen's Items step, formatted once when the step is built.</summary>
/// <param name="Name">The item's name, or "+n more" on the last row when the drops don't all fit.</param>
/// <param name="IconFile">The item's icon; null on the "+n more" row.</param>
/// <param name="Quantity">
/// "×n" for a stack; null for a gear piece, one row each (decision 345), and the "+n more" row.
/// </param>
internal sealed record VictoryDropRow(string Name, string? IconFile, MeasuredText? Quantity);
