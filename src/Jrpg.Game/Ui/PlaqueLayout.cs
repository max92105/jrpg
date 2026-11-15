using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// Where a plaque sits around a measured text (decision 646): hugging it with a little room on each side, centered on a
/// given x, its top where asked.
/// </summary>
/// <param name="Plaque">The plaque's rectangle on the virtual screen.</param>
/// <param name="TextPosition">The text's top left, inside the padding.</param>
internal readonly record struct PlaqueLayout(Rectangle Plaque, Vector2 TextPosition)
{
    // The clip's plaque: about 100 x 36 px around "Attack" with a 24 px line.
    private const int PaddingX = 14;
    private const int PaddingY = 6;

    /// <param name="textSize">The text's measured width and line height, in pixels.</param>
    /// <param name="centerX">Where the plaque's middle goes across.</param>
    /// <param name="top">Where the plaque's top goes.</param>
    public static PlaqueLayout Around(Vector2 textSize, int centerX, int top)
    {
        var width = (int)Math.Ceiling(textSize.X) + (2 * PaddingX);
        var height = (int)Math.Ceiling(textSize.Y) + (2 * PaddingY);
        var left = centerX - (width / 2);
        return new PlaqueLayout(new Rectangle(left, top, width, height), new Vector2(left + PaddingX, top + PaddingY));
    }
}
