using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The pixels of a horizontal gradient, one row, for a texture stretched over a strip (legacy <c>LinearGradientBrush</c>
/// from left to right). Built once per texture.
/// </summary>
internal static class GradientStrip
{
    /// <param name="stops">At least one, in increasing offset.</param>
    /// <param name="width">Pixels in the row; each takes the color at its center.</param>
    public static Color[] Colors(IReadOnlyList<GradientStop> stops, int width)
    {
        var colors = new Color[width];
        for (var x = 0; x < width; x++)
        {
            colors[x] = ColorAt((x + 0.5f) / width, stops);
        }

        return colors;
    }

    // Past either end, the nearest stop's color holds, as in WPF.
    private static Color ColorAt(float offset, IReadOnlyList<GradientStop> stops)
    {
        if (offset <= stops[0].Offset)
        {
            return stops[0].Color;
        }

        for (var index = 1; index < stops.Count; index++)
        {
            var (left, right) = (stops[index - 1], stops[index]);
            if (offset <= right.Offset)
            {
                return Color.Lerp(left.Color, right.Color, (offset - left.Offset) / (right.Offset - left.Offset));
            }
        }

        return stops[^1].Color;
    }
}
