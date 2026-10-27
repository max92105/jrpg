using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A section's header, as the reference's Status page draws them: a dark band across the section with the label in it,
/// so each section reads as its own block (decision 335).
/// </summary>
internal sealed class SectionTitle
{
    /// <summary>The band's height, in pixels; pages place their content from its bottom.</summary>
    public const int Height = 36;

    // Placeholders: a 22 px label in the primary color, 12 px in from the band's left edge.
    private const int LabelPixels = 22;
    private const int LabelInset = 12;

    private readonly SpriteFontBase _font;
    private readonly string _label;

    /// <param name="text">The label's message and font.</param>
    /// <param name="messageId">The label's message.</param>
    public SectionTitle(ScreenText text, string messageId)
    {
        _font = text.Fonts.FontAt(LabelPixels);
        _label = text.Messages.Text(messageId);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="section">The band's top left and width; the band is <see cref="Height"/> tall.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        batch.Draw(textures.Pixel, new Rectangle(section.X, section.Y, section.Width, Height), UiTheme.SectionBand * opacity);
        var labelTop = section.Y + ((Height - _font.LineHeight) / 2f);
        batch.DrawString(_font, _label, new Vector2(section.X + LabelInset, labelTop), UiTheme.TextPrimary * opacity);
    }
}
