namespace Jrpg.Editor.GifImport;

/// <summary>
/// Makes a GIF's frames whole by its disposal rules: each patch drawn over the canvas where it lies, clipped to the
/// canvas, a transparent pixel leaving what is under it; after a frame, <see cref="GifDisposal.Keep"/> leaves the
/// canvas, <see cref="GifDisposal.ClearToBackground"/> clears that frame's rectangle to transparent, and
/// <see cref="GifDisposal.RestorePrevious"/> puts the canvas back as it was before the frame.
/// </summary>
internal static class GifFrameComposer
{
    /// <returns>One canvas per frame, row by row, BGRA packed as the frames' pixels.</returns>
    public static IReadOnlyList<uint[]> Compose(GifImage image)
    {
        var canvas = new uint[image.Width * image.Height];
        var composed = new List<uint[]>(image.Frames.Count);
        foreach (var frame in image.Frames)
        {
            var before = frame.Disposal == GifDisposal.RestorePrevious ? (uint[])canvas.Clone() : null;
            DrawPatch(canvas, image, frame);
            composed.Add((uint[])canvas.Clone());
            if (frame.Disposal == GifDisposal.ClearToBackground)
            {
                ClearPatch(canvas, image, frame);
            }

            canvas = before ?? canvas;
        }

        return composed;
    }

    private static void DrawPatch(uint[] canvas, GifImage image, GifFrame frame)
    {
        var area = ClippedArea(image, frame);
        for (var y = area.Top; y < area.Bottom; y++)
        {
            for (var x = area.Left; x < area.Right; x++)
            {
                var pixel = frame.Pixels[((y - frame.Top) * frame.Width) + (x - frame.Left)];
                if (!IsTransparent(pixel))
                {
                    canvas[(y * image.Width) + x] = pixel;
                }
            }
        }
    }

    private static void ClearPatch(uint[] canvas, GifImage image, GifFrame frame)
    {
        var area = ClippedArea(image, frame);
        for (var y = area.Top; y < area.Bottom; y++)
        {
            Array.Clear(canvas, (y * image.Width) + area.Left, area.Right - area.Left);
        }
    }

    // The part of the patch that lies on the canvas; empty (Right equal to Left, Bottom to Top) when none does, as a
    // patch wholly past the canvas's edge, which WPF reads as stored.
    private static (int Left, int Top, int Right, int Bottom) ClippedArea(GifImage image, GifFrame frame)
    {
        var left = Math.Min(frame.Left, image.Width);
        var top = Math.Min(frame.Top, image.Height);
        var right = Math.Clamp(frame.Left + frame.Width, left, image.Width);
        var bottom = Math.Clamp(frame.Top + frame.Height, top, image.Height);
        return (left, top, right, bottom);
    }

    private static bool IsTransparent(uint pixel) => pixel >> 24 == 0;
}
