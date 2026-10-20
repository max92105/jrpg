using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A font for text drawn straight on a picture, with the legacy soft dark outline (<c>Effect.TextOutline</c>: black,
/// blur 6) under it so it stays readable on any background.
/// </summary>
internal sealed class OutlinedFont(SpriteFontBase font)
{
    private const int OutlineBlur = 6;

    public SpriteFontBase Font => font;

    /// <param name="batch">Already begun.</param>
    /// <param name="text">Formatted when its value changed, never per frame.</param>
    /// <param name="position">The text's top left.</param>
    /// <param name="color">Already faded; the outline fades with it.</param>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color)
    {
        var fade = color.A / 255f;
        batch.DrawString(font, text, position, UiTheme.TextOutline * fade, effect: FontSystemEffect.Blurry, effectAmount: OutlineBlur);
        batch.DrawString(font, text, position, color);
    }
}
