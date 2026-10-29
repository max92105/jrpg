using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>One line of the victory screen's Items step, formatted once when the step is built.</summary>
/// <param name="Name">The item's name, or "+n more" on the last row when the drops don't all fit.</param>
/// <param name="IconFile">The item's icon; null on the "+n more" row.</param>
/// <param name="Tag">
/// The right-aligned text: "×n" for a stack, the rarity's name for a piece above Common; null for a plain piece,
/// one row each (decision 345), and the "+n more" row.
/// </param>
internal sealed record VictoryDropRow(string Name, string? IconFile, MeasuredText? Tag)
{
    /// <summary>
    /// A piece's rarity, which its name and tag are drawn in (decision 362); Common for a stack and the "+n more" row.
    /// </summary>
    public Rarity Rarity { get; init; }

    /// <summary>
    /// Whether it is a stack of a crafting material, whose name and count are drawn in the material color.
    /// </summary>
    public bool IsMaterial { get; init; }
}
