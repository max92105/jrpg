using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws a <see cref="StatusSheet"/>: the member's profile and experience beside a diamond-framed portrait, then their
/// parameters and what they wear.
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

    // Rows, down from the page's top margin: the profile, then each section's title and its lines, 18 px apart. The
    // equipment and the resources share a row, in two columns; the skills follow, a name over its description each.
    private const int ClassTop = 42;
    private const int ExperienceTop = 84;
    private const int ParametersTop = PortraitSize + SectionGap;
    private const int EquipmentTop = ParametersTop + SectionHeight + (3 * StatLineHeight) + SectionGap;
    private const int SectionHeight = 36;
    private const int SectionGap = 18;
    private const int ValueBelowLabel = 26;
    private const int SkillPitch = 60;
    private const int DescriptionBelowName = 30;

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
    private readonly SectionTitle _resourcesTitle;
    private readonly SectionTitle _skillsTitle;
    private readonly StatBar _xpBar;
    private StatusSheet? _sheet;

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
        _resourcesTitle = new SectionTitle(text, ui, MessageIds.StatusResources);
        _skillsTitle = new SectionTitle(text, ui, MessageIds.StatusSkills);
        Vitals = new MemberVitalBars(text, ui, BodyPixels);
        _xpBar = new StatBar(ui, UiTheme.XpBar);
    }

    /// <summary>The HP meter and its bar; show the member's vitals on it before building their sheet.</summary>
    public MemberVitalBars Vitals { get; }

    /// <summary>Draws <paramref name="sheet"/> from now on, its experience bar filled to match.</summary>
    public void Show(StatusSheet sheet)
    {
        _sheet = sheet;
        _xpBar.Show(sheet.Experience.Earned, sheet.Experience.Needed);
    }

    /// <exception cref="InvalidOperationException">No sheet was shown yet.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var sheet = _sheet ?? throw new InvalidOperationException("Show a sheet before drawing it.");
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var portraitLeft = area.Right - Margin - PortraitSize;
        var profileWidth = portraitLeft - PortraitGap - left;
        var fullWidth = area.Right - Margin - left;
        var page = new PageFrame(batch, textures, sheet, opacity);

        DrawPortrait(page, new Vector2(portraitLeft + (PortraitSize / 2f), top + (PortraitSize / 2f)));
        page.Write(_headingFont, sheet.Name, new Vector2(left, top), UiTheme.TextPrimary);
        page.Write(_labelFont, sheet.ClassName, new Vector2(left, top + ClassTop), UiTheme.TextSecondary);
        DrawExperience(page, new Rectangle(left, top + ExperienceTop, profileWidth, 0));
        DrawParameters(page, new Rectangle(left, top + ParametersTop, fullWidth, 0));

        var halfWidth = (fullWidth - ColumnGap) / 2;
        DrawEquipment(page, new Rectangle(left, top + EquipmentTop, halfWidth, 0));
        DrawResources(page, new Rectangle(left + halfWidth + ColumnGap, top + EquipmentTop, halfWidth, 0));
        var sharedRows = Math.Max(sheet.Worn.Length, sheet.Resources.Length);
        var skillsTop = top + EquipmentTop + SectionHeight + (sharedRows * StatLineHeight) + SectionGap;
        DrawSkills(page, new Rectangle(left, skillsTop, fullWidth, area.Bottom - skillsTop));
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

    // HP alone on the first line with its bar under it, then attack and defense side by side, then speed alone.
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

        var speedTop = section.Y + SectionHeight + (2 * StatLineHeight);
        DrawLine(page, new Vector2(section.X + Indent, speedTop), _labels.Speed, page.Sheet.Speed);

        // HP is the first line, so its bar goes under it.
        var hpBar = new Rectangle(section.X + Indent, section.Y + SectionHeight + VitalBarTop, columnWidth, BarHeight);
        Vitals.Hp.Bar.Draw(page.Batch, page.Textures, hpBar, page.Opacity);
    }

    private void DrawResources(PageFrame page, Rectangle section)
    {
        _resourcesTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        for (var index = 0; index < page.Sheet.Resources.Length; index++)
        {
            var lineTop = section.Y + SectionHeight + (index * StatLineHeight);
            var resource = page.Sheet.Resources[index];
            DrawLine(page, new Vector2(section.X + Indent, lineTop), resource.Name, resource.Detail);
        }
    }

    // As many skills as the page's height holds; the rest wait for a scrolling list.
    private void DrawSkills(PageFrame page, Rectangle section)
    {
        _skillsTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var fitting = Math.Min(page.Sheet.Skills.Length, (section.Height - SectionHeight) / SkillPitch);
        for (var index = 0; index < fitting; index++)
        {
            var nameTop = section.Y + SectionHeight + (index * SkillPitch);
            var skill = page.Sheet.Skills[index];
            page.Write(_bodyFont, skill.Name, new Vector2(section.X + Indent, nameTop), UiTheme.TextPrimary);
            page.Write(_labelFont, skill.Detail, new Vector2(section.X + Indent, nameTop + DescriptionBelowName), UiTheme.TextSecondary);
        }
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

    // What the view draws with this frame, so the drawing helpers stay at a few parameters.
    private readonly record struct PageFrame(SpriteBatch Batch, TextureLibrary Textures, StatusSheet Sheet, float Opacity)
    {
        public void Write(SpriteFontBase font, string text, Vector2 position, Color color) =>
            Batch.DrawString(font, text, position, color * Opacity);
    }
}
