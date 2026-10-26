using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws a <see cref="StatusSheet"/>: the profile and experience beside a diamond-framed portrait, then the
/// parameters and what the member wears.
/// </summary>
internal sealed class StatusSheetView
{
    // Placeholder layout (decision 82): a 28 x 20 margin, 30 px headings, 26 px body text, 18 px labels and captions; a
    // 240 px portrait 32 px right of the profile; 46 px stat lines in two columns 40 px apart, 10 px in under titles.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 30;
    private const int BodyPixels = 26;
    private const int LabelPixels = 18;
    private const int PortraitSize = 240;
    private const int PortraitFrame = 2;
    private const int PortraitGap = 32;
    private const int Indent = 10;
    private const int StatLineHeight = 46;
    private const int ColumnGap = 40;
    private const int ValueOffset = 200;

    // Rows, down from the page's top margin: the profile, then each section's title and its lines.
    private const int ClassTop = 42;
    private const int ExperienceTop = 84;
    private const int ParametersTop = PortraitSize + 18;
    private const int EquipmentTop = ParametersTop + SectionHeight + (2 * StatLineHeight) + 18;
    private const int SectionHeight = 36;
    private const int ValueBelowLabel = 26;

    // The experience bar: 6 px tall, 12 px under the experience values, 16 px short of the profile's right edge. HP
    // gets the same slim bar under its line.
    private const int BarHeight = 6;
    private const int XpBarTop = ValueBelowLabel + 44;
    private const int XpBarRightGap = 16;
    private const int VitalBarTop = 34;

    private readonly UiTextures _ui;
    private readonly StatusLabels _labels;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _labelFont;
    private readonly SectionTitle _experienceTitle;
    private readonly SectionTitle _parametersTitle;
    private readonly SectionTitle _equipmentTitle;
    private readonly StatBar _xpBar;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines and baked portraits.</param>
    /// <param name="labels">The captions, in the sheet's order.</param>
    public StatusSheetView(ScreenText text, UiTextures ui, StatusLabels labels)
    {
        _ui = ui;
        _labels = labels;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _experienceTitle = new SectionTitle(text, ui, MessageIds.StatusExperience);
        _parametersTitle = new SectionTitle(text, ui, MessageIds.StatusParameters);
        _equipmentTitle = new SectionTitle(text, ui, MessageIds.StatusEquipment);
        Vitals = new MemberVitalBars(text, ui, BodyPixels);
        _xpBar = new StatBar(ui, UiTheme.XpBar);
    }

    /// <summary>
    /// The shown member's HP meter: shown the member before their sheet is built, since the sheet reads its text.
    /// </summary>
    public MemberVitalBars Vitals { get; }

    /// <summary>Sets the experience bar to the sheet's member, when the shown member changes.</summary>
    public void Show(StatusSheet sheet) => _xpBar.Show(sheet.Experience.Earned, sheet.Experience.Needed);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel, section lines and portraits come from.</param>
    /// <param name="place">The sheet, and the page's area.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var area = place.Area;
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var portraitLeft = area.Right - Margin - PortraitSize;
        var profileWidth = portraitLeft - PortraitGap - left;
        var fullWidth = area.Right - Margin - left;
        var page = new PageFrame(batch, textures, opacity, place.Sheet);

        DrawPortrait(page, new Vector2(portraitLeft + (PortraitSize / 2f), top + (PortraitSize / 2f)));
        page.Write(_headingFont, place.Sheet.Name, new Vector2(left, top), UiTheme.TextPrimary);
        page.Write(_labelFont, place.Sheet.ClassName, new Vector2(left, top + ClassTop), UiTheme.TextSecondary);
        DrawExperience(page, new Rectangle(left, top + ExperienceTop, profileWidth, 0));
        DrawParameters(page, new Rectangle(left, top + ParametersTop, fullWidth, 0));
        DrawEquipment(page, new Rectangle(left, top + EquipmentTop, fullWidth, 0));
    }

    // A diamond like the dialogue's, framed by a slightly larger pale diamond behind it.
    private void DrawPortrait(PageFrame page, Vector2 center)
    {
        Diamond.WithDiagonal(center, PortraitSize + (2 * PortraitFrame)).Draw(page.Batch, page.Textures.Pixel, UiTheme.BarLine * page.Opacity);
        var portrait = _ui.DiamondPortrait(page.Textures, page.Sheet.PortraitFile, PortraitSize);
        var corner = new Vector2(center.X - (PortraitSize / 2f), center.Y - (PortraitSize / 2f));
        page.Batch.Draw(portrait, corner, Color.White * page.Opacity);
    }

    // Level, experience and experience to the next level, each a small label over its value, in three columns.
    private void DrawExperience(PageFrame page, Rectangle section)
    {
        _experienceTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var columnWidth = (section.Width - Indent) / 3;
        var labelsTop = section.Y + SectionHeight;
        for (var column = 0; column < 3; column++)
        {
            var columnLeft = section.X + Indent + (column * columnWidth);
            page.Write(_labelFont, _labels.Progress[column], new Vector2(columnLeft, labelsTop), UiTheme.TextSecondary);
            page.Write(_headingFont, page.Sheet.Progress[column], new Vector2(columnLeft, labelsTop + ValueBelowLabel), UiTheme.TextPrimary);
        }

        var xpBar = new Rectangle(section.X + Indent, labelsTop + XpBarTop, section.Width - Indent - XpBarRightGap, BarHeight);
        _xpBar.Draw(page.Batch, page.Textures, xpBar, page.Opacity);
    }

    // HP alone on the first line with its bar under it, then attack and defense side by side, in two columns.
    private void DrawParameters(PageFrame page, Rectangle section)
    {
        _parametersTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var columnWidth = (section.Width - Indent - ColumnGap) / 2;
        for (var index = 0; index < _labels.Stats.Length; index++)
        {
            var (column, line) = index == 0 ? (0, 0) : ((index - 1) % 2, 1 + ((index - 1) / 2));
            var columnLeft = section.X + Indent + (column * (columnWidth + ColumnGap));
            var lineTop = section.Y + SectionHeight + (line * StatLineHeight);
            DrawLine(page, new Vector2(columnLeft, lineTop), _labels.Stats[index], page.Sheet.Stats[index]);
        }

        // HP is the first line, so its bar goes under it.
        var hpBar = new Rectangle(section.X + Indent, section.Y + SectionHeight + VitalBarTop, columnWidth, BarHeight);
        Vitals.Hp.Bar.Draw(page.Batch, page.Textures, hpBar, page.Opacity);
    }

    private void DrawEquipment(PageFrame page, Rectangle section)
    {
        _equipmentTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        for (var index = 0; index < _labels.Slots.Length; index++)
        {
            var lineTop = section.Y + SectionHeight + (index * StatLineHeight);
            DrawLine(page, new Vector2(section.X + Indent, lineTop), _labels.Slots[index], page.Sheet.Worn[index]);
        }
    }

    private void DrawLine(PageFrame page, Vector2 position, string label, string value)
    {
        page.Write(_bodyFont, label, position, UiTheme.TextSecondary);
        page.Write(_bodyFont, value, position + new Vector2(ValueOffset, 0), UiTheme.TextPrimary);
    }

    /// <summary>What the view draws, and where.</summary>
    /// <param name="Sheet">The shown member's sheet.</param>
    /// <param name="Area">The page's area.</param>
    public readonly record struct Place(StatusSheet Sheet, Rectangle Area);

    // What the view draws with this frame, so the drawing helpers stay at a few parameters.
    private readonly record struct PageFrame(SpriteBatch Batch, TextureLibrary Textures, float Opacity, StatusSheet Sheet)
    {
        public void Write(SpriteFontBase font, string text, Vector2 position, Color color) =>
            Batch.DrawString(font, text, position, color * Opacity);
    }
}
