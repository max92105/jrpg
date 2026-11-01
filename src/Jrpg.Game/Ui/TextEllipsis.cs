using FontStashSharp;

namespace Jrpg.Game.Ui;

/// <summary>Shortens a line to a width, ending it with three dots, so a list row never runs past its column.</summary>
internal static class TextEllipsis
{
    private const string Dots = "...";

    /// <returns>
    /// <paramref name="text"/> when it fits in <paramref name="width"/>, else its longest start that fits with the
    /// dots.
    /// </returns>
    public static string Fit(SpriteFontBase font, string text, float width)
    {
        if (font.MeasureString(text).X <= width)
        {
            return text;
        }

        for (var length = text.Length - 1; length > 0; length--)
        {
            var shortened = text[..length].TrimEnd() + Dots;
            if (font.MeasureString(shortened).X <= width)
            {
                return shortened;
            }
        }

        return Dots;
    }
}
