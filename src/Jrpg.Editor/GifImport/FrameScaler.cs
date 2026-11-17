namespace Jrpg.Editor.GifImport;

/// <summary>
/// Scales whole frames to a height in pixels, aspect kept, resampled once (decision 637): nearest neighbor when the
/// factor or its inverse is a whole number, so pixel art stays crisp; otherwise each pixel is the average of the source
/// pixels it covers, weighted by their alpha so a transparent edge doesn't darken the color beside it. A battle
/// background is always averaged when shrunk, whatever the factor (decision 904): a halved painting keeps its detail as
/// color, not dropped pixels.
/// </summary>
internal static class FrameScaler
{
    private const int ChannelMax = 255;

    /// <returns>The scaled width, rounded to the nearest pixel, at least 1.</returns>
    public static int WidthAt(int width, int height, int targetHeight) =>
        Math.Max(1, (int)Math.Round((double)width * targetHeight / height, MidpointRounding.AwayFromZero));

    /// <summary>Whether scaling to the height is a whole factor up or down, so it keeps pixel art crisp.</summary>
    public static bool IsWholeFactor(int height, int targetHeight) => targetHeight % height == 0 || height % targetHeight == 0;

    /// <param name="frames">Whole frames, row by row, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <param name="width">Each frame's width.</param>
    /// <param name="height">Each frame's height.</param>
    /// <param name="targetHeight">The height wanted, at least 1.</param>
    /// <returns>
    /// The frames at <see cref="WidthAt"/> by the target height; the same frames when the size is unchanged.
    /// </returns>
    public static IReadOnlyList<uint[]> Scale(IReadOnlyList<uint[]> frames, int width, int height, int targetHeight)
    {
        var target = (Width: WidthAt(width, height, targetHeight), Height: targetHeight);
        if (target == (width, height))
        {
            return frames;
        }

        var source = (Width: width, Height: height);
        return IsWholeFactor(height, targetHeight)
            ? [.. frames.Select(frame => Nearest(frame, source, target))]
            : [.. frames.Select(frame => Averaged(frame, source, target))];
    }

    /// <summary>The alpha-weighted average whatever the factor: a battle background's shrink (decision 904).</summary>
    /// <param name="frames">Whole frames, row by row, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <param name="width">Each frame's width.</param>
    /// <param name="height">Each frame's height.</param>
    /// <param name="targetHeight">The height wanted, at least 1.</param>
    /// <returns>The frames at <see cref="WidthAt"/> by the target height.</returns>
    public static IReadOnlyList<uint[]> ScaleAveraged(IReadOnlyList<uint[]> frames, int width, int height, int targetHeight)
    {
        var target = (Width: WidthAt(width, height, targetHeight), Height: targetHeight);
        return [.. frames.Select(frame => Averaged(frame, (width, height), target))];
    }

    private static uint[] Nearest(uint[] frame, (int Width, int Height) source, (int Width, int Height) target)
    {
        var scaled = new uint[target.Width * target.Height];
        for (var y = 0; y < target.Height; y++)
        {
            var sourceRow = y * source.Height / target.Height * source.Width;
            for (var x = 0; x < target.Width; x++)
            {
                scaled[(y * target.Width) + x] = frame[sourceRow + (x * source.Width / target.Width)];
            }
        }

        return scaled;
    }

    private static uint[] Averaged(uint[] frame, (int Width, int Height) source, (int Width, int Height) target)
    {
        var scaled = new uint[target.Width * target.Height];
        for (var y = 0; y < target.Height; y++)
        {
            for (var x = 0; x < target.Width; x++)
            {
                var covered = (
                    Left: EdgeAt(x, source.Width, target.Width),
                    Top: EdgeAt(y, source.Height, target.Height),
                    Right: EdgeAt(x + 1, source.Width, target.Width),
                    Bottom: EdgeAt(y + 1, source.Height, target.Height));
                scaled[(y * target.Width) + x] = AverageOver(frame, source.Width, covered);
            }
        }

        return scaled;
    }

    // The integer product is divided last, so the last edge is exactly the source's size: a step multiplied back can
    // land a hair past it, and the average would then read past the frame.
    private static double EdgeAt(int index, int sourceSize, int targetSize) => (double)index * sourceSize / targetSize;

    // Each source pixel counts by the share of it the area covers, and its color by its alpha too.
    private static uint AverageOver(uint[] frame, int width, (double Left, double Top, double Right, double Bottom) area)
    {
        double weight = 0, alpha = 0, red = 0, green = 0, blue = 0;
        for (var y = (int)area.Top; y < area.Bottom; y++)
        {
            var rowShare = Math.Min(area.Bottom, y + 1) - Math.Max(area.Top, y);
            for (var x = (int)area.Left; x < area.Right; x++)
            {
                var share = rowShare * (Math.Min(area.Right, x + 1) - Math.Max(area.Left, x));
                var pixel = frame[(y * width) + x];
                var opacity = share * (pixel >> 24) / ChannelMax;
                weight += share;
                alpha += opacity;
                red += opacity * ((pixel >> 16) & 0xFF);
                green += opacity * ((pixel >> 8) & 0xFF);
                blue += opacity * (pixel & 0xFF);
            }
        }

        return alpha == 0 ? 0 : Packed(alpha / weight, red / alpha, green / alpha, blue / alpha);
    }

    private static uint Packed(double alpha, double red, double green, double blue) =>
        (Channel(alpha * ChannelMax) << 24) | (Channel(red) << 16) | (Channel(green) << 8) | Channel(blue);

    private static uint Channel(double value) =>
        (uint)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, ChannelMax);
}
