using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// Cuts a picture into a diamond, the frame the reference UI gives portraits (<c>docs/ui_ux_exemples</c>): the centered
/// square of the image, shrunk to <c>size</c>, with the corners outside the diamond made transparent. Done once per
/// picture, since SpriteBatch can't clip to a diamond.
/// </summary>
internal static class DiamondCrop
{
    /// <param name="source">Premultiplied pixels, row by row.</param>
    /// <param name="width">The source's width.</param>
    /// <param name="height">The source's height.</param>
    /// <param name="size">The baked texture's side; the diamond touches the middle of each side.</param>
    public static Color[] Bake(Color[] source, int width, int height, int size)
    {
        var side = Math.Min(width, height);
        var left = (width - side) / 2;
        var top = (height - side) / 2;
        var pixels = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var coverage = Coverage(x, y, size);
                if (coverage > 0)
                {
                    var (blockLeft, blockWidth) = SourceSpan(x, side, size);
                    var (blockTop, blockHeight) = SourceSpan(y, side, size);
                    var block = new Rectangle(left + blockLeft, top + blockTop, blockWidth, blockHeight);
                    pixels[(y * size) + x] = Average(source, width, block) * coverage;
                }
            }
        }

        return pixels;
    }

    // The source pixels baked pixel `index` stands for along one axis. Each span ends where the next begins, so shrinking
    // by an uneven ratio (1024 to 200) skips none; it holds at least one, so a picture smaller than the portrait grows.
    private static (int Start, int Length) SourceSpan(int index, int side, int size)
    {
        var start = index * side / size;
        var end = Math.Max((index + 1) * side / size, start + 1);
        return (start, end - start);
    }

    // How much of the pixel the diamond covers, judged at its center, with a one-pixel soft edge against jaggies.
    private static float Coverage(int x, int y, int size)
    {
        var half = size / 2f;
        var distance = Math.Abs(x + 0.5f - half) + Math.Abs(y + 0.5f - half);
        return Math.Clamp(half - distance + 0.5f, 0, 1);
    }

    // A box filter: each baked pixel is the mean of the source pixels it stands for, so a large portrait shrinks smoothly.
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
