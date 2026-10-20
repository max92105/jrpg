using FontStashSharp;

namespace Jrpg.Game.Ui;

/// <summary>A piece of text and its width in pixels, measured once, for right-aligning or centering it.</summary>
internal readonly record struct MeasuredText(string Text, float Width)
{
    public static MeasuredText Of(SpriteFontBase font, string text) => new(text, font.MeasureString(text).X);
}
