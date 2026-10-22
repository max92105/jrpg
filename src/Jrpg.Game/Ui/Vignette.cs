using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The pixels of a radial gradient centered on an area, for a texture stretched over it. Built once per texture.
/// </summary>
internal static class Vignette
{
    // Radii of 0.75 (placeholder): the gradient's offset 1 lies 0.75 of the width (or height) from the center,
    // so the corners stay short of the darkest stop.
    private const float Radius = 0.75f;

    /// <param name="stops">Along the radius: 0 at the center to 1 at the ellipse.</param>
    /// <param name="width">Pixels across; each takes the color at its center.</param>
    /// <param name="height">Pixels down.</param>
    /// <returns>Row by row, top to bottom.</returns>
    public static Color[] Colors(IReadOnlyList<GradientStop> stops, int width, int height)
    {
        var colors = new Color[width * height];
        for (var y = 0; y < height; y++)
        {
            var dy = ((y + 0.5f) / height) - 0.5f;
            for (var x = 0; x < width; x++)
            {
                var dx = ((x + 0.5f) / width) - 0.5f;
                var offset = MathF.Sqrt((dx * dx) + (dy * dy)) / Radius;
                colors[(y * width) + x] = GradientStrip.ColorAt(offset, stops);
            }
        }

        return colors;
    }
}
