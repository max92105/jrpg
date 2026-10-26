using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A list's column captions ("ITEM … QTY") in spaced caps, like the reference's small labels, over a hairline that
/// fades out to the right; measured once.
/// </summary>
internal sealed class ColumnTitles
{
    /// <summary>The titles' row, from the captions' top to where the list's rows start (placeholder).</summary>
    public const int Height = 36;

    // Placeholders: the hairline 26 px below the captions' top, letters 2 px apart as the battle's resource labels.
    private const int LineGap = 26;
    private const float LetterSpacing = 2;

    private readonly SpriteFontBase _font;
    private readonly UiTextures _ui;
    private readonly IReadOnlyList<ColumnTitle> _titles;
    private readonly MeasuredText[] _captions;

    /// <param name="font">The captions' font.</param>
    /// <param name="ui">Where the hairline's texture is made, once.</param>
    /// <param name="titles">The captions, left to right.</param>
    public ColumnTitles(SpriteFontBase font, UiTextures ui, IReadOnlyList<ColumnTitle> titles)
    {
        _font = font;
        _ui = ui;
        _titles = titles;
        _captions = [.. titles.Select(title => CapsOf(font, title.Caption))];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the hairline's texture is made.</param>
    /// <param name="section">The titles' row: the captions' top left, and the hairline's width. The height isn't read.</param>
    /// <param name="opacity">0 to 1, while the page fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        var color = UiTheme.TextSecondary * opacity;
        for (var index = 0; index < _titles.Count; index++)
        {
            var (title, caption) = (_titles[index], _captions[index]);
            var left = title.IsFromRight ? section.Right - title.Offset - caption.Width : section.X + title.Offset;
            batch.DrawString(_font, caption.Text, new Vector2(left, section.Y), color, characterSpacing: LetterSpacing);
        }

        batch.Draw(_ui.SectionLine(textures), new Rectangle(section.X, section.Y + LineGap, section.Width, 1), Color.White * opacity);
    }

    private static MeasuredText CapsOf(SpriteFontBase font, string caption)
    {
        var caps = caption.ToUpperInvariant();
        return new MeasuredText(caps, font.MeasureString(caps, characterSpacing: LetterSpacing).X);
    }
}
