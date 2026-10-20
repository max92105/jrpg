using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// Shrinks a picture's centered square smoothly to a small square, once per picture: the 1024 px art drawn at 26 or 40 px
/// with point sampling would alias badly.
/// </summary>
internal static class SquareShrink
{
    /// <param name="source">Premultiplied pixels, row by row.</param>
    /// <param name="width">The source's width.</param>
    /// <param name="height">The source's height.</param>
    /// <param name="size">The baked square's side.</param>
    /// <returns><paramref name="size"/> x <paramref name="size"/> premultiplied pixels, row by row.</returns>
    public static Color[] Bake(Color[] source, int width, int height, int size)
    {
        var side = Math.Min(width, height);
        var left = (width - side) / 2;
        var top = (height - side) / 2;
        var pixels = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            var (blockTop, blockHeight) = SourceSpan(y, side, size);
            for (var x = 0; x < size; x++)
            {
                var (blockLeft, blockWidth) = SourceSpan(x, side, size);
                pixels[(y * size) + x] = Average(source, width, new Rectangle(left + blockLeft, top + blockTop, blockWidth, blockHeight));
            }
        }

        return pixels;
    }

    // The source pixels baked pixel `index` stands for along one axis. Each span ends where the next begins, so shrinking
    // by an uneven ratio (1024 to 200) skips none; it holds at least one, so a picture smaller than the square grows.
    private static (int Start, int Length) SourceSpan(int index, int side, int size)
    {
        var start = index * side / size;
        var end = Math.Max((index + 1) * side / size, start + 1);
        return (start, end - start);
    }

    // A box filter: each baked pixel is the mean of the source pixels it stands for.
    private static Color Average(Color[] source, int width, Rectangle block)
    {
        var (red, green, blue, alpha) = (0, 0, 0, 0);
        for (var y = block.Top; y < block.Bottom; y++)
        {
            for (var x = block.Left; x < block.Right; x++)
            {
                var color = source[(y * width) + x];
                (red, green, blue, alpha) = (red + color.R, green + color.G, blue + color.B, alpha + color.A);
            }
        }

        var count = block.Width * block.Height;
        return new Color(red / count, green / count, blue / count, alpha / count);
    }
}
