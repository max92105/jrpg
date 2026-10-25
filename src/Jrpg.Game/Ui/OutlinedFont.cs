using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A font for text drawn straight on a picture, with a soft dark outline (black, blur 6) under it so it stays readable
/// on any background.
/// </summary>
internal sealed class OutlinedFont(SpriteFontBase font)
{
    private const int OutlineBlur = 6;

    public SpriteFontBase Font => font;

    /// <param name="batch">Already begun.</param>
    /// <param name="text">Formatted when its value changed, never per frame.</param>
    /// <param name="position">The text's top left.</param>
    /// <param name="color">Already faded; the outline fades with it.</param>
    /// <param name="characterSpacing">Extra pixels between letters, for spaced caps; 0 for plain text.</param>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color, float characterSpacing = 0)
    {
        var fade = color.A / 255f;
        batch.DrawString(
            font, text, position, UiTheme.TextOutline * fade, characterSpacing: characterSpacing, effect: FontSystemEffect.Blurry, effectAmount: OutlineBlur);
        batch.DrawString(font, text, position, color, characterSpacing: characterSpacing);
    }

    /// <summary>Draws <paramref name="text"/> centered on a point and scaled about it, such as a number popping.</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="text">Measured in this font, when its value changed.</param>
    /// <param name="center">Where the text's middle goes.</param>
    /// <param name="look">The color, already faded, and the size (1 is the font's own).</param>
    public void DrawCentered(SpriteBatch batch, MeasuredText text, Vector2 center, (Color Color, float Scale) look)
    {
        var origin = new Vector2(text.Width / 2, font.LineHeight / 2f);
        var scale = new Vector2(look.Scale);
        var outline = UiTheme.TextOutline * (look.Color.A / 255f);
        batch.DrawString(font, text.Text, center, outline, origin: origin, scale: scale, effect: FontSystemEffect.Blurry, effectAmount: OutlineBlur);
        batch.DrawString(font, text.Text, center, look.Color, origin: origin, scale: scale);
    }
}
