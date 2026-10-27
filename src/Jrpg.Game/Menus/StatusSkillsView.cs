using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Status page's Skills section under its dark band: each skill's name with its description wrapped under it
/// and a hairline below, as many whole skills as fit; the page doesn't scroll.
/// </summary>
internal sealed class StatusSkillsView
{
    // Placeholder layout (decisions 82, 334): the first name 10 px under the band, 24 px names, 18 px descriptions 30
    // px under the name, 12 px in, a hairline 8 px under the description, then a 10 px gap.
    private const int NamePixels = 24;
    private const int DescriptionPixels = 18;
    private const int BelowBand = SectionTitle.Height + 10;
    private const int Indent = 12;
    private const int DescriptionBelowName = 30;
    private const int LineBelowDescription = 8;
    private const int SkillGap = 10;

    private readonly SectionTitle _title;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _descriptionFont;
    private StatusSheet? _wrappedSheet;
    private int _wrappedWidth;
    private RichTextLayout[] _descriptions = [];

    /// <param name="text">Messages and fonts.</param>
    public StatusSkillsView(ScreenText text)
    {
        _title = new SectionTitle(text, MessageIds.StatusSkills);
        _nameFont = text.Fonts.FontAt(NamePixels);
        _descriptionFont = text.Fonts.FontAt(DescriptionPixels);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">The sheet, and the section: its title's top left, down to the page's bottom.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var (sheet, section) = place;
        _title.Draw(batch, textures, section, opacity);
        var left = section.X + Indent;
        var width = section.Width - Indent;
        Wrap(sheet, width);
        var nameTop = section.Y + BelowBand;
        for (var index = 0; index < sheet.Skills.Count; index++)
        {
            var description = _descriptions[index];
            var lineTop = nameTop + DescriptionBelowName + description.Size.Y + LineBelowDescription;
            if (lineTop > section.Bottom)
            {
                return;
            }

            batch.DrawString(_nameFont, sheet.Skills[index].Name, new Vector2(left, nameTop), UiTheme.TextPrimary * opacity);
            description.Draw(batch, new Vector2(left, nameTop + DescriptionBelowName), UiTheme.TextSecondary * opacity);
            batch.Draw(textures.Pixel, new Rectangle(left, lineTop, width, 1), UiTheme.Stroke * opacity);
            nameTop = lineTop + 1 + SkillGap;
        }
    }

    // Wrapped again only when the shown sheet or the width changes, so drawing lays out nothing new each frame.
    private void Wrap(StatusSheet sheet, int width)
    {
        if (ReferenceEquals(sheet, _wrappedSheet) && width == _wrappedWidth)
        {
            return;
        }

        _wrappedSheet = sheet;
        _wrappedWidth = width;
        _descriptions = [.. sheet.Skills.Select(skill => new RichTextLayout
        {
            // Off, so a bracket or a slash in content text is never read as a FontStashSharp formatting command.
            SupportsCommands = false,
            Font = _descriptionFont,
            Width = width,
            Text = skill.Text,
        })];
    }

    /// <summary>What the section draws, and where.</summary>
    /// <param name="Sheet">The shown member's sheet.</param>
    /// <param name="Section">The title's top left and the width; the skills stop at its bottom.</param>
    public readonly record struct Place(StatusSheet Sheet, Rectangle Section);
}
