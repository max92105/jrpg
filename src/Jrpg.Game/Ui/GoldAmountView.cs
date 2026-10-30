using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws an amount of gold as the coin icon, then the number (decision 380: no currency name). The coin is the font's
/// line height tall, so the pair reads as one word wherever a screen places it.
/// </summary>
internal sealed class GoldAmountView
{
    // The gap between the coin and the number (placeholder).
    private const int CoinGap = 6;

    private readonly SpriteFontBase _font;
    private readonly UiTextures _ui;
    private readonly int _coinSize;

    /// <param name="font">The number's font, which the caller measures the amount with.</param>
    /// <param name="ui">Where the coin icon is shrunk to the font's size.</param>
    public GoldAmountView(SpriteFontBase font, UiTextures ui)
    {
        _font = font;
        _ui = ui;
        _coinSize = font.LineHeight;
    }

    /// <summary>How wide the coin and <paramref name="amount"/> are together.</summary>
    public float WidthOf(MeasuredText amount) => _coinSize + CoinGap + amount.Width;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the coin icon comes from.</param>
    /// <param name="place">The measured amount, where its coin's top left goes, and the number's color.</param>
    /// <param name="opacity">The screen's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        batch.Draw(_ui.Icon(textures, UiArtFiles.Icons.Gold, _coinSize), place.TopLeft, Color.White * opacity);
        batch.DrawString(_font, place.Amount.Text, place.TopLeft + new Vector2(_coinSize + CoinGap, 0), place.Color * opacity);
    }

    /// <summary>An amount as drawn.</summary>
    /// <param name="Amount">The number, measured with the view's font.</param>
    /// <param name="TopLeft">The coin's top left; the number follows on the same line.</param>
    /// <param name="Color">The number's color.</param>
    public readonly record struct Place(MeasuredText Amount, Vector2 TopLeft, Color Color);
}
