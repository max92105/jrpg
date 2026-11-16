using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Animation;

/// <summary>
/// The strip Strip from sheet cells writes (decision 990): the picked cells of one sheet side by side, each kept at its
/// size and padded to a square, played at one pace.
/// </summary>
/// <param name="FileName">The strip's file name in the effects folder, ending in ".png".</param>
/// <param name="CellSize">A cell's size in the sheet's pixels, both at least 1.</param>
/// <param name="Cells">
/// The cells in play order, column and row from 0, a cell repeating where it was picked again.
/// </param>
/// <param name="FrameMilliseconds">The strip's pace, at least 1.</param>
internal sealed record CellStrip(
    string FileName, (int Width, int Height) CellSize, IReadOnlyList<SpriteCell> Cells, int FrameMilliseconds);
