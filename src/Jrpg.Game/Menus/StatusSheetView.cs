using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws a <see cref="StatusSheet"/>: the member's profile, level and experience beside a diamond-framed portrait, then
/// their parameters, what they wear, their resources and their skills. Each row reads like the reference's: a small
/// spaced-caps label, the value large on the column's right edge, a hairline under it (docs/ui-reference).
/// </summary>
internal sealed class StatusSheetView
{
    // Placeholder layout (decision 82): a 28 x 20 margin, 30 px headings and values, 26 px body text, 18 px labels and
    // captions spaced 2 px; a 240 px portrait 32 px right of the profile; 46 px rows in two columns 40 px apart, 10 px
    // in under titles.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 30;
    private const int BodyPixels = 26;
    private const int LabelPixels = 18;
    private const float LabelSpacing = 2;
    private const int PortraitSize = 240;
    private const int PortraitFrame = 2;
    private const int PortraitGap = 32;
    private const int Indent = 10;
    private const int RowHeight = 46;
    private const int ColumnGap = 40;
    private const int LevelGap = 12;

    // Rows, down from the page's top margin: the profile, then each section's title and its rows, 18 px apart. The
    // equipment and the resources share a row, in two columns; the skills follow, a name over its description each.
    private const int ClassTop = 42;
    private const int ExperienceTop = 84;
    private const int ParametersTop = PortraitSize + SectionGap;
    private const int EquipmentTop = ParametersTop + SectionHeight + (3 * RowHeight) + SectionGap;
    private const int SectionHeight = 36;
    private const int SectionGap = 18;
    private const int ValueBelowLabel = 26;
    private const int SkillPitch = 60;
    private const int DescriptionBelowName = 30;

    // The experience bar: 6 px tall, 12 px under the experience values, 16 px short of the profile's right edge. HP
    // gets the same slim bar under its value, above the row's hairline.
    private const int BarHeight = 6;
    private const int XpBarTop = ValueBelowLabel + 44;
    private const int XpBarRightGap = 16;
    private const int VitalBarTop = 37;

    private readonly UiTextures _ui;
    private readonly StatusLabels _labels;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _labelFont;
    private readonly string[] _parameterLabels;
    private readonly MeasuredText _levelLabel;
    private readonly SectionTitle _experienceTitle;
    private readonly SectionTitle _parametersTitle;
    private readonly SectionTitle _equipmentTitle;
    private readonly SectionTitle _resourcesTitle;
    private readonly SectionTitle _skillsTitle;
    private readonly StatBar _xpBar;
    private ShownSheet? _shown;

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
        _parameterLabels = [.. labels.Stats, labels.Speed];
        _levelLabel = new MeasuredText(labels.Progress[0], _labelFont.MeasureString(labels.Progress[0], characterSpacing: LabelSpacing).X);
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

    /// <summary>
    /// Draws <paramref name="sheet"/> from now on, its experience bar filled to match; its values are measured here, so
    /// drawing measures nothing.
    /// </summary>
    public void Show(StatusSheet sheet)
    {
        _xpBar.Show(sheet.Experience.Earned, sheet.Experience.Needed);
        _shown = new ShownSheet(
            sheet,
            Measure(sheet.Progress[0]),
            [.. sheet.Stats.Append(sheet.Speed).Select(Measure)],
            [.. sheet.Worn.Select(Measure)],
            [.. sheet.Resources.Select(resource => Measure(resource.Detail))]);
    }

    /// <exception cref="InvalidOperationException">No sheet was shown yet.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var shown = _shown ?? throw new InvalidOperationException("Show a sheet before drawing it.");
        var sheet = shown.Sheet;
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var portraitLeft = area.Right - Margin - PortraitSize;
        var profileWidth = portraitLeft - PortraitGap - left;
        var fullWidth = area.Right - Margin - left;
        var page = new PageFrame(batch, textures, shown, opacity);

        DrawPortrait(page, new Vector2(portraitLeft + (PortraitSize / 2f), top + (PortraitSize / 2f)));
        page.Write(_headingFont, sheet.Name, new Vector2(left, top), UiTheme.TextPrimary);
        DrawLevel(page, new Vector2(left + profileWidth - XpBarRightGap, top));
        page.Write(_labelFont, sheet.ClassName, new Vector2(left, top + ClassTop), UiTheme.TextSecondary);
        DrawExperience(page, new Rectangle(left, top + ExperienceTop, profileWidth, 0));
        DrawParameters(page, new Rectangle(left, top + ParametersTop, fullWidth, 0));

        var halfWidth = (fullWidth - ColumnGap) / 2;
        DrawEquipment(page, new Rectangle(left, top + EquipmentTop, halfWidth, 0));
        DrawResources(page, new Rectangle(left + halfWidth + ColumnGap, top + EquipmentTop, halfWidth, 0));
        var sharedRows = Math.Max(sheet.Worn.Length, sheet.Resources.Length);
        var skillsTop = top + EquipmentTop + SectionHeight + (sharedRows * RowHeight) + SectionGap;
        DrawSkills(page, new Rectangle(left, skillsTop, fullWidth, area.Bottom - skillsTop));
    }

    private MeasuredText Measure(string value) => MeasuredText.Of(_headingFont, value);

    // A diamond like the dialogue's, framed by a slightly larger pale diamond behind it.
    private void DrawPortrait(PageFrame page, Vector2 center)
    {
        Diamond.WithDiagonal(center, PortraitSize + (2 * PortraitFrame)).Draw(page.Batch, page.Textures.Pixel, UiTheme.BarLine * page.Opacity);
        var portrait = _ui.DiamondPortrait(page.Textures, page.Shown.Sheet.PortraitFile, PortraitSize);
        var corner = new Vector2(center.X - (PortraitSize / 2f), center.Y - (PortraitSize / 2f));
        page.Batch.Draw(portrait, corner, Color.White * page.Opacity);
    }

    // On the name's line, ending at the profile's right edge: the small label, then the level large.
    private void DrawLevel(PageFrame page, Vector2 topRight)
    {
        var level = page.Shown.Level;
        page.Write(_headingFont, level.Text, topRight - new Vector2(level.Width, 0), UiTheme.TextPrimary);
        var labelLeft = topRight.X - level.Width - LevelGap - _levelLabel.Width;
        DrawLabel(page, _levelLabel.Text, new Vector2(labelLeft, topRight.Y + _headingFont.LineHeight - _labelFont.LineHeight));
    }

    // Experience and experience to the next level, each a small label over its value, in two columns; the level is
    // beside the name.
    private void DrawExperience(PageFrame page, Rectangle section)
    {
        _experienceTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var columnWidth = (section.Width - Indent) / 2;
        var labelsTop = section.Y + SectionHeight;
        for (var column = 0; column < 2; column++)
        {
            var columnLeft = section.X + Indent + (column * columnWidth);
            DrawLabel(page, _labels.Progress[column + 1], new Vector2(columnLeft, labelsTop));
            page.Write(_headingFont, page.Shown.Sheet.Progress[column + 1], new Vector2(columnLeft, labelsTop + ValueBelowLabel), UiTheme.TextPrimary);
        }

        var xpBar = new Rectangle(section.X + Indent, labelsTop + XpBarTop, section.Width - Indent - XpBarRightGap, BarHeight);
        _xpBar.Draw(page.Batch, page.Textures, xpBar, page.Opacity);
    }

    // In the labels' order (HP, attack, defense, speed): HP alone on the first row with its bar under it, attack and
    // defense side by side, then speed alone.
    private void DrawParameters(PageFrame page, Rectangle section)
    {
        _parametersTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var columnWidth = (section.Width - Indent - ColumnGap) / 2;
        for (var index = 0; index < _parameterLabels.Length; index++)
        {
            var (column, line) = index switch
            {
                0 => (0, 0),
                3 => (0, 2),
                _ => (index - 1, 1),
            };
            var columnLeft = section.X + Indent + (column * (columnWidth + ColumnGap));
            var row = new Rectangle(columnLeft, section.Y + SectionHeight + (line * RowHeight), columnWidth, RowHeight);
            DrawRow(page, row, _parameterLabels[index], page.Shown.Stats[index]);
        }

        // HP is the first row, so its bar goes under its value.
        var hpBar = new Rectangle(section.X + Indent, section.Y + SectionHeight + VitalBarTop, columnWidth, BarHeight);
        Vitals.Hp.Bar.Draw(page.Batch, page.Textures, hpBar, page.Opacity);
    }

    private void DrawEquipment(PageFrame page, Rectangle section)
    {
        _equipmentTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        for (var index = 0; index < _labels.Slots.Length; index++)
        {
            DrawRow(page, RowAt(section, index), _labels.Slots[index], page.Shown.Worn[index]);
        }
    }

    private void DrawResources(PageFrame page, Rectangle section)
    {
        _resourcesTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        for (var index = 0; index < page.Shown.Sheet.Resources.Length; index++)
        {
            DrawRow(page, RowAt(section, index), page.Shown.Sheet.Resources[index].Name, page.Shown.Resources[index]);
        }
    }

    // As many skills as the page's height holds; the rest wait for a scrolling list.
    private void DrawSkills(PageFrame page, Rectangle section)
    {
        _skillsTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var fitting = Math.Min(page.Shown.Sheet.Skills.Length, (section.Height - SectionHeight) / SkillPitch);
        for (var index = 0; index < fitting; index++)
        {
            var row = new Rectangle(section.X + Indent, section.Y + SectionHeight + (index * SkillPitch), section.Width - Indent, SkillPitch);
            var skill = page.Shown.Sheet.Skills[index];
            page.Write(_bodyFont, skill.Name, new Vector2(row.X, row.Y), UiTheme.TextPrimary);
            page.Write(_labelFont, skill.Detail, new Vector2(row.X, row.Y + DescriptionBelowName), UiTheme.TextSecondary);
            DrawHairline(page, row);
        }
    }

    private static Rectangle RowAt(Rectangle section, int index) =>
        new(section.X + Indent, section.Y + SectionHeight + (index * RowHeight), section.Width - Indent, RowHeight);

    // The label small and spaced at the left, its foot on the value's line; the value large on the row's right edge.
    private void DrawRow(PageFrame page, Rectangle row, string label, MeasuredText value)
    {
        DrawLabel(page, label, new Vector2(row.X, row.Y + _headingFont.LineHeight - _labelFont.LineHeight));
        page.Write(_headingFont, value.Text, new Vector2(row.Right - value.Width, row.Y), UiTheme.TextPrimary);
        DrawHairline(page, row);
    }

    private void DrawLabel(PageFrame page, string label, Vector2 position) =>
        page.Batch.DrawString(_labelFont, label, position, UiTheme.TextSecondary * page.Opacity, characterSpacing: LabelSpacing);

    private static void DrawHairline(PageFrame page, Rectangle row) =>
        page.Batch.Draw(page.Textures.Pixel, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), UiTheme.Stroke * page.Opacity);

    // What the view draws with this frame, so the drawing helpers stay at a few parameters.
    private readonly record struct PageFrame(SpriteBatch Batch, TextureLibrary Textures, ShownSheet Shown, float Opacity)
    {
        public void Write(SpriteFontBase font, string text, Vector2 position, Color color) =>
            Batch.DrawString(font, text, position, color * Opacity);
    }

    // The shown sheet and its values, measured in the value font when it was shown: the level, the parameters with
    // speed last, what's worn and the resources' amounts, each in the sheet's order.
    private sealed record ShownSheet(StatusSheet Sheet, MeasuredText Level, MeasuredText[] Stats, MeasuredText[] Worn, MeasuredText[] Resources);
}
