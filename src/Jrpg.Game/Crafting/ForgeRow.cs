using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>
/// One owned piece as the Forge lists it, its affixes formatted once (decision 361's lines, one per affix, so the
/// detail column draws them as rows).
/// </summary>
/// <param name="Piece">The piece, as the inventory holds it.</param>
/// <param name="Name">Its name, measured so the rarity can stand beside it.</param>
/// <param name="RarityName">Its rarity as the player reads it.</param>
/// <param name="AffixLines">In roll order; empty for a plain piece.</param>
internal sealed record ForgeRow(GearPiece Piece, MeasuredText Name, string RarityName, IReadOnlyList<string> AffixLines);
