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
    /// <summary>The descriptions' type size.</summary>
    internal const int DescriptionPixels = 18;

    /// <summary>How far the names and descriptions sit in from the section's left.</summary>
    internal const int Indent = 12;

    // Placeholder layout (decisions 82, 334, 373): the first name 4 px under the band, 24 px names, 18 px descriptions
    // 28 px under the name, 12 px in, a hairline 6 px under the description, then a 7 px gap. Tight enough that three
    // skills with two-line descriptions fit beside Resources (decision 373).
    private const int NamePixels = 24;
    private const int BelowBand = SectionTitle.Height + 4;
    private const int DescriptionBelowName = 28;
    private const int LineBelowDescription = 6;
    private const int SkillGap = 7;

    // Two lines of the 18 px type, whose line height is 18: the layout test measures the real descriptions.
    private const int TwoLineDescription = 36;

    /// <summary>
    /// The room three skills with two-line descriptions take, down to under the last hairline: what the page keeps for
    /// the section (decision 373).
    /// </summary>
    internal const int RoomForThreeSkills =
        BelowBand + (3 * (DescriptionBelowName + TwoLineDescription + LineBelowDescription)) + (2 * (1 + SkillGap)) + 1;

    private readonly SectionTitle _title;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _descriptionFont;
    private StatusSheet? _wrappedSheet;
    private int _wrappedWidth;
    private RichTextLayout[] _descriptions = [];
    private int[] _hairlineTops = [];

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
        for (var index = 0; index < sheet.Skills.Count && _hairlineTops[index] <= section.Height; index++)
        {
            var nameTop = section.Y + NameTopOf(_hairlineTops, index);
            batch.DrawString(_nameFont, sheet.Skills[index].Name, new Vector2(left, nameTop), UiTheme.TextPrimary * opacity);
            _descriptions[index].Draw(batch, new Vector2(left, nameTop + DescriptionBelowName), UiTheme.TextSecondary * opacity);
            batch.Draw(textures.Pixel, new Rectangle(left, section.Y + _hairlineTops[index], width, 1), UiTheme.Stroke * opacity);
        }
    }

    /// <summary>
    /// Each skill's hairline, from the section's top, given its description's height: a skill shows only when its
    /// hairline is within the section, so the page shows whole skills.
    /// </summary>
    /// <param name="descriptionHeights">Each description's height as wrapped, in the skills' order.</param>
    internal static int[] HairlineTopsOf(IReadOnlyList<int> descriptionHeights)
    {
        var hairlineTops = new int[descriptionHeights.Count];
        for (var index = 0; index < hairlineTops.Length; index++)
        {
            hairlineTops[index] = NameTopOf(hairlineTops, index) + DescriptionBelowName + descriptionHeights[index] + LineBelowDescription;
        }

        return hairlineTops;
    }

    // The first name sits under the band; each next one under the hairline before it and the gap.
    private static int NameTopOf(int[] hairlineTops, int index) => index == 0 ? BelowBand : hairlineTops[index - 1] + 1 + SkillGap;

    // Wrapped again only when the shown sheet or the width changes, so drawing lays out nothing new each frame.
    private void Wrap(StatusSheet sheet, int width)
    {
        if (ReferenceEquals(sheet, _wrappedSheet) && width == _wrappedWidth)
        {
            return;
        }

        _wrappedSheet = sheet;
        _wrappedWidth = width;
        _descriptions = [.. sheet.Skills.Select(skill => DescriptionLayout(_descriptionFont, width, skill.Text))];
        _hairlineTops = HairlineTopsOf([.. _descriptions.Select(description => description.Size.Y)]);
    }

    /// <summary>A description wrapped at the column's width, as the section draws it.</summary>
    /// <param name="font">The descriptions' font, at <see cref="DescriptionPixels"/>.</param>
    /// <param name="width">The text's width: the section's, less <see cref="Indent"/>.</param>
    /// <param name="text">The description.</param>
    internal static RichTextLayout DescriptionLayout(SpriteFontBase font, int width, string text) => new()
    {
        // Off, so a bracket or a slash in content text is never read as a FontStashSharp formatting command.
        SupportsCommands = false,
        Font = font,
        Width = width,
        Text = text,
    };

    /// <summary>What the section draws, and where.</summary>
    /// <param name="Sheet">The shown member's sheet.</param>
    /// <param name="Section">The title's top left and the width; the skills stop at its bottom.</param>
    public readonly record struct Place(StatusSheet Sheet, Rectangle Section);
}
