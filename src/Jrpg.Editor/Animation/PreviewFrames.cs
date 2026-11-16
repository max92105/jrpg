using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Animation;

/// <summary>
/// The images a <see cref="FramePreview"/> plays, cut once when its host chooses what to preview, so a render tick only
/// changes which one shows.
/// </summary>
internal static class PreviewFrames
{
    private const int BytesPerPixel = 4;
    private const double Dpi = 96;

    /// <summary>
    /// A strip's frames left to right, each its width over <paramref name="frameCount"/> and its whole height; none
    /// when the width doesn't split into that many, as a frame cut short would show a neighbor's edge.
    /// </summary>
    public static IReadOnlyList<BitmapSource> OfStrip(BitmapSource strip, int frameCount)
    {
        if (frameCount <= 0 || strip.PixelWidth % frameCount != 0)
        {
            return [];
        }

        var width = strip.PixelWidth / frameCount;
        return
        [
            .. Enumerable.Range(0, frameCount)
                .Select(frame => new CroppedBitmap(strip, new Int32Rect(frame * width, 0, width, strip.PixelHeight))),
        ];
    }

    /// <summary>
    /// A sheet's picked cells in pick order (decision 990), a cell picked twice cut twice; a cell past the sheet's edge
    /// is skipped, as its crop would throw.
    /// </summary>
    /// <param name="sheet">The sheet in its own pixels.</param>
    /// <param name="cell">A cell's size in the sheet's pixels.</param>
    /// <param name="cells">The cells in play order, column and row from 0.</param>
    public static IReadOnlyList<BitmapSource> OfCells(
        BitmapSource sheet, (int Width, int Height) cell, IReadOnlyList<SpriteCell> cells) =>
    [
        .. cells
            .Select(picked => new Int32Rect(picked.Column * cell.Width, picked.Row * cell.Height, cell.Width, cell.Height))
            .Where(area => area is { X: >= 0, Y: >= 0 }
                && area.Width <= sheet.PixelWidth - area.X && area.Height <= sheet.PixelHeight - area.Y)
            .Select(area => new CroppedBitmap(sheet, area)),
    ];

    /// <summary>Whole frames of pixels, as a GIF is composed for its import (decision 987), each one image.</summary>
    /// <param name="frames">Frames of one size, row by row, BGRA packed little-endian (alpha in the top byte).</param>
    /// <param name="width">Each frame's width in pixels.</param>
    /// <param name="height">Each frame's height in pixels.</param>
    public static IReadOnlyList<BitmapSource> OfPixels(IReadOnlyList<uint[]> frames, int width, int height) =>
        [.. frames.Select(pixels => ImageOf(pixels, width, height))];

    // Frozen, as an image that never changes needs no watching by WPF.
    private static BitmapSource ImageOf(uint[] pixels, int width, int height)
    {
        var image = BitmapSource.Create(width, height, Dpi, Dpi, PixelFormats.Bgra32, null, pixels, width * BytesPerPixel);
        image.Freeze();
        return image;
    }
}
