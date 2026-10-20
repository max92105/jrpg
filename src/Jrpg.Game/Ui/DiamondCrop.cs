using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// Cuts a picture into a diamond, the frame the reference UI gives portraits (<c>docs/ui_ux_exemples</c>): the centered
/// square of the image, shrunk to <c>size</c> (<see cref="SquareShrink"/>), with the corners outside the diamond made
/// transparent. Done once per picture, since SpriteBatch can't clip to a diamond.
/// </summary>
internal static class DiamondCrop
{
    /// <param name="source">Premultiplied pixels, row by row.</param>
    /// <param name="width">The source's width.</param>
    /// <param name="height">The source's height.</param>
    /// <param name="size">The baked texture's side; the diamond touches the middle of each side.</param>
    public static Color[] Bake(Color[] source, int width, int height, int size)
    {
        var pixels = SquareShrink.Bake(source, width, height, size);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                pixels[(y * size) + x] *= Coverage(x, y, size);
            }
        }

        return pixels;
    }

    // How much of the pixel the diamond covers, judged at its center, with a one-pixel soft edge against jaggies.
    private static float Coverage(int x, int y, int size)
    {
        var half = size / 2f;
        var distance = Math.Abs(x + 0.5f - half) + Math.Abs(y + 0.5f - half);
        return Math.Clamp(half - distance + 0.5f, 0, 1);
    }
}
