using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>A list's column captions ("Name … Qty") over a hairline that fades out to the right, measured once.</summary>
internal sealed class ColumnTitles
{
    /// <summary>The titles' row, from the captions' top to where the list's rows start (placeholder).</summary>
    public const int Height = 36;

    // The hairline sits 26 px below the captions' top (placeholder).
    private const int LineGap = 26;

    private readonly SpriteFontBase _font;
    private readonly UiTextures _ui;
    private readonly IReadOnlyList<ColumnTitle> _titles;

    /// <param name="font">The captions' font, which measured them.</param>
    /// <param name="ui">Where the hairline's texture is made, once.</param>
    /// <param name="titles">The captions, left to right.</param>
    public ColumnTitles(SpriteFontBase font, UiTextures ui, IReadOnlyList<ColumnTitle> titles)
    {
        _font = font;
        _ui = ui;
        _titles = titles;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the hairline's texture is made.</param>
    /// <param name="section">The titles' row: the captions' top left, and the hairline's width. The height isn't read.</param>
    /// <param name="opacity">0 to 1, while the page fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        var color = UiTheme.TextSecondary * opacity;
        foreach (var title in _titles)
        {
            var left = title.IsFromRight ? section.Right - title.Offset - title.Text.Width : section.X + title.Offset;
            batch.DrawString(_font, title.Text.Text, new Vector2(left, section.Y), color);
        }

        batch.Draw(_ui.SectionLine(textures), new Rectangle(section.X, section.Y + LineGap, section.Width, 1), Color.White * opacity);
    }
}
