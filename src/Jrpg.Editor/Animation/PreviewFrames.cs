using System.Windows;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.Animation;

/// <summary>
/// The images a <see cref="FramePreview"/> plays, cut once when its host chooses what to preview, so a render tick only
/// changes which one shows.
/// </summary>
internal static class PreviewFrames
{
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
}
