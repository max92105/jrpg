using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A section's header: a small label over a hairline that fades out to the right.
/// </summary>
internal sealed class SectionTitle
{
    /// <summary>How far below the label's top the hairline sits, in pixels; pages place their content from it.</summary>
    public const int LineGap = 20;

    private const int LabelPixels = 14;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly string _label;

    /// <param name="text">The label's message and font.</param>
    /// <param name="ui">Where the hairline's texture is made, once.</param>
    /// <param name="messageId">The label's message.</param>
    public SectionTitle(ScreenText text, UiTextures ui, string messageId)
    {
        _ui = ui;
        _font = text.Fonts.FontAt(LabelPixels);
        _label = text.Messages.Text(messageId);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the hairline's texture is made.</param>
    /// <param name="section">The label's top left; its width is the hairline's. The height isn't read.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        batch.DrawString(_font, _label, new Vector2(section.X, section.Y), UiTheme.TextSecondary * opacity);
        var line = new Rectangle(section.X, section.Y + LineGap, section.Width, 1);
        batch.Draw(_ui.SectionLine(textures), line, Color.White * opacity);
    }
}
