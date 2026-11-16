using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Animation;

/// <summary>
/// The picked cells of a sheet cut out as frames of pixels (decision 990), the frames
/// <see cref="GifImport.FrameStrip"/> writes. WPF imaging needs an STA thread.
/// </summary>
internal static class SheetCells
{
    private const int BytesPerPixel = 4;

    /// <summary>A frame per cell in pick order, a cell picked twice copied twice.</summary>
    /// <param name="sheet">The sheet in its own pixels, in any pixel format.</param>
    /// <param name="cell">A cell's size in the sheet's pixels, both at least 1.</param>
    /// <param name="cells">The cells in play order, column and row from 0.</param>
    /// <returns>Each cell's pixels row by row, BGRA packed as <see cref="GifImport.GifFrame.Pixels"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A cell lies past the sheet's edge; the tool's problem line rules that out before OK.
    /// </exception>
    public static IReadOnlyList<uint[]> Cut(BitmapSource sheet, (int Width, int Height) cell, IReadOnlyList<SpriteCell> cells)
    {
        var bgra = new FormatConvertedBitmap(sheet, PixelFormats.Bgra32, null, 0);
        return [.. cells.Select(picked => PixelsOf(bgra, AreaOf(picked, cell, (bgra.PixelWidth, bgra.PixelHeight))))];
    }

    // The edges are products, in long so a far cell can't wrap round into the sheet, and compared as "left > width -
    // cell" so the bound itself can't overflow.
    private static Int32Rect AreaOf(SpriteCell picked, (int Width, int Height) cell, (int Width, int Height) sheet)
    {
        var (left, top) = ((long)picked.Column * cell.Width, (long)picked.Row * cell.Height);
        if (left < 0 || top < 0 || left > sheet.Width - cell.Width || top > sheet.Height - cell.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(picked), $"Cell ({picked.Column}, {picked.Row}) lies past the {sheet.Width} x {sheet.Height} sheet.");
        }

        return new Int32Rect((int)left, (int)top, cell.Width, cell.Height);
    }

    private static uint[] PixelsOf(BitmapSource bgra, Int32Rect area)
    {
        var pixels = new uint[area.Width * area.Height];
        bgra.CopyPixels(area, pixels, area.Width * BytesPerPixel, 0);
        return pixels;
    }
}
