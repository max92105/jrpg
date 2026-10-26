using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws a <see cref="StatusSheet"/>: the profile, level and experience beside a diamond-framed portrait, then the
/// parameters, what the member wears beside their class resources, and their skills. Each row reads like the
/// reference's: a small spaced-caps label, a large value on the column's right edge, a hairline under it.
/// </summary>
internal sealed class StatusSheetView
{
    // Placeholder layout (decision 82): a 28 x 20 margin; 30 px name, 18 px class, 16 px labels 2 px apart, 32 px
    // values (SZ1's large numbers); a 240 px portrait 32 px right of the profile; 50 px rows in two columns 40 px
    // apart, 10 px in under titles.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int NamePixels = 30;
    private const int ClassPixels = 18;
    private const int LabelPixels = 16;
    private const int ValuePixels = 32;
    private const float LabelSpacing = 2;
    private const int PortraitSize = 240;
    private const int PortraitFrame = 2;
    private const int PortraitGap = 32;
    private const int Indent = 10;
    private const int RowHeight = 50;
    private const int ColumnGap = 40;

    // Rows, down from the page's top margin: the profile, then each section's title and its rows. Equipment and the
    // resources share a row, half the width each; the skills fill what is left of the page.
    private const int LevelGap = 24;
    private const int ClassTop = 42;
    private const int ExperienceTop = 84;
    private const int ParametersTop = PortraitSize + 18;
    private const int EquipmentTop = ParametersTop + SectionHeight + (2 * RowHeight) + 18;
    private const int SkillsTop = EquipmentTop + SectionHeight + (2 * RowHeight) + 18;
    private const int SectionHeight = 36;
    private const int ValueBelowLabel = 22;

    // The experience bar: 6 px tall, under the experience values, 16 px short of the profile's right edge. HP's bar
    // takes its row's hairline.
    private const int BarHeight = 6;
    private const int XpBarTop = ValueBelowLabel + 48;
    private const int XpBarRightGap = 16;

    // Where each parameter sits, in the order the values come (HP, Attack, Defense, Speed): HP and Speed on the first
    // line, Attack and Defense on the second.
    private static readonly (int Column, int Line)[] _parameterCells = [(0, 0), (0, 1), (1, 1), (1, 0)];

    private readonly UiTextures _ui;
    private readonly StatusLabels _labels;
    private readonly string[] _parameterLabels;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _classFont;
    private readonly SpriteFontBase _labelFont;
    private readonly SpriteFontBase _valueFont;
    private readonly float _levelLabelWidth;
    private readonly SectionTitle _experienceTitle;
    private readonly SectionTitle _parametersTitle;
    private readonly SectionTitle _equipmentTitle;
    private readonly SectionTitle _resourcesTitle;
    private readonly StatusSkillsView _skills;
    private readonly StatBar _xpBar;
    private MeasuredSheet? _shown;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines and baked portraits.</param>
    /// <param name="labels">The captions, in the sheet's order.</param>
    public StatusSheetView(ScreenText text, UiTextures ui, StatusLabels labels)
    {
        _ui = ui;
        _labels = labels;
        _parameterLabels = [.. labels.Stats, labels.Speed];
        _nameFont = text.Fonts.FontAt(NamePixels);
        _classFont = text.Fonts.FontAt(ClassPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _valueFont = text.Fonts.FontAt(ValuePixels);
        _levelLabelWidth = _labelFont.MeasureString(labels.Progress[0], characterSpacing: LabelSpacing).X;
        _experienceTitle = new SectionTitle(text, ui, MessageIds.StatusExperience);
        _parametersTitle = new SectionTitle(text, ui, MessageIds.StatusParameters);
        _equipmentTitle = new SectionTitle(text, ui, MessageIds.StatusEquipment);
        _resourcesTitle = new SectionTitle(text, ui, MessageIds.StatusResources);
        _skills = new StatusSkillsView(text, ui);
        Vitals = new MemberVitalBars(text, ui, ValuePixels);
        _xpBar = new StatBar(ui, UiTheme.XpBar);
    }

    /// <summary>
    /// The shown member's HP meter: shown the member before their sheet is built, since the sheet reads its text.
    /// </summary>
    public MemberVitalBars Vitals { get; }

    /// <summary>Measures the sheet's values and sets the experience bar, when the shown member changes.</summary>
    public void Show(StatusSheet sheet)
    {
        _xpBar.Show(sheet.Experience.Earned, sheet.Experience.Needed);
        _shown = new MeasuredSheet(
            _nameFont.MeasureString(sheet.Name).X,
            [.. sheet.Stats.Append(sheet.Speed).Select(Measured)],
            [.. sheet.Worn.Select(Measured)],
            [.. sheet.Resources.Select(resource => resource.Name)],
            [.. sheet.Resources.Select(resource => Measured(resource.Text))]);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel, section lines and portraits come from.</param>
    /// <param name="place">The sheet, and the page's area.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    /// <exception cref="InvalidOperationException">No sheet was shown yet.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var shown = _shown ?? throw new InvalidOperationException("Show a sheet before drawing it.");
        var area = place.Area;
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var portraitLeft = area.Right - Margin - PortraitSize;
        var fullWidth = area.Right - Margin - left;
        var halfWidth = (fullWidth - ColumnGap) / 2;
        var page = new PageFrame(batch, textures, opacity, place.Sheet);

        DrawPortrait(page, new Vector2(portraitLeft + (PortraitSize / 2f), top + (PortraitSize / 2f)));
        page.Write(_nameFont, place.Sheet.Name, new Vector2(left, top), UiTheme.TextPrimary);
        DrawLevel(page, new Vector2(left + shown.NameWidth + LevelGap, top));
        page.Write(_classFont, place.Sheet.ClassName, new Vector2(left, top + ClassTop), UiTheme.TextSecondary);
        DrawExperience(page, new Rectangle(left, top + ExperienceTop, portraitLeft - PortraitGap - left, 0));
        DrawParameters(page, new Rectangle(left, top + ParametersTop, fullWidth, 0), shown.Parameters);
        DrawRows(page, new Rectangle(left, top + EquipmentTop, halfWidth, 0), new RowsSection(_equipmentTitle, _labels.Slots, shown.Worn));
        var resources = new RowsSection(_resourcesTitle, shown.ResourceNames, shown.Resources);
        DrawRows(page, new Rectangle(left + halfWidth + ColumnGap, top + EquipmentTop, halfWidth, 0), resources);
        var skills = new Rectangle(left, top + SkillsTop, fullWidth, area.Bottom - (top + SkillsTop));
        _skills.Draw(batch, textures, new StatusSkillsView.Place(place.Sheet, skills), opacity);
    }

    private MeasuredText Measured(string value) => MeasuredText.Of(_valueFont, value);

    // A diamond like the dialogue's, framed by a slightly larger pale diamond behind it.
    private void DrawPortrait(PageFrame page, Vector2 center)
    {
        Diamond.WithDiagonal(center, PortraitSize + (2 * PortraitFrame)).Draw(page.Batch, page.Textures.Pixel, UiTheme.BarLine * page.Opacity);
        var portrait = _ui.DiamondPortrait(page.Textures, page.Sheet.PortraitFile, PortraitSize);
        var corner = new Vector2(center.X - (PortraitSize / 2f), center.Y - (PortraitSize / 2f));
        page.Batch.Draw(portrait, corner, Color.White * page.Opacity);
    }

    // "LV" small, then the level large, beside the name; the label centered on the number's line.
    private void DrawLevel(PageFrame page, Vector2 position)
    {
        var labelTop = position.Y + ((_valueFont.LineHeight - _labelFont.LineHeight) / 2f);
        page.WriteLabel(_labelFont, _labels.Progress[0], new Vector2(position.X, labelTop));
        page.Write(_valueFont, page.Sheet.Progress[0], position + new Vector2(_levelLabelWidth + Indent, 0), UiTheme.TextPrimary);
    }

    // Experience and experience to the next level, each a small label over its value, in two columns, over the bar.
    private void DrawExperience(PageFrame page, Rectangle section)
    {
        _experienceTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var columnWidth = (section.Width - Indent) / 2;
        var labelsTop = section.Y + SectionHeight;
        for (var column = 0; column < 2; column++)
        {
            var columnLeft = section.X + Indent + (column * columnWidth);
            page.WriteLabel(_labelFont, _labels.Progress[column + 1], new Vector2(columnLeft, labelsTop));
            page.Write(_valueFont, page.Sheet.Progress[column + 1], new Vector2(columnLeft, labelsTop + ValueBelowLabel), UiTheme.TextPrimary);
        }

        var xpBar = new Rectangle(section.X + Indent, labelsTop + XpBarTop, section.Width - Indent - XpBarRightGap, BarHeight);
        _xpBar.Draw(page.Batch, page.Textures, xpBar, page.Opacity);
    }

    // HP and speed on the first line, attack and defense on the second; HP's bar takes the place of its hairline.
    private void DrawParameters(PageFrame page, Rectangle section, MeasuredText[] values)
    {
        _parametersTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var columnWidth = (section.Width - Indent - ColumnGap) / 2;
        var firstLineTop = section.Y + SectionHeight;
        for (var index = 0; index < values.Length; index++)
        {
            var (column, line) = _parameterCells[index];
            var columnLeft = section.X + Indent + (column * (columnWidth + ColumnGap));
            DrawRow(page, new Rectangle(columnLeft, firstLineTop + (line * RowHeight), columnWidth, RowHeight), _parameterLabels[index], values[index]);
        }

        var hpBar = new Rectangle(section.X + Indent, firstLineTop + RowHeight - BarHeight, columnWidth, BarHeight);
        Vitals.Hp.Bar.Draw(page.Batch, page.Textures, hpBar, page.Opacity);
    }

    private void DrawRows(PageFrame page, Rectangle section, RowsSection rows)
    {
        rows.Title.Draw(page.Batch, page.Textures, section, page.Opacity);
        for (var index = 0; index < rows.Values.Count; index++)
        {
            var row = new Rectangle(section.X + Indent, section.Y + SectionHeight + (index * RowHeight), section.Width - Indent, RowHeight);
            DrawRow(page, row, rows.Labels[index], rows.Values[index]);
        }
    }

    // The label small on the left, the value large on the right edge, both centered on the row; a hairline under it.
    private void DrawRow(PageFrame page, Rectangle row, string label, MeasuredText value)
    {
        var middle = row.Y + (row.Height / 2f);
        page.WriteLabel(_labelFont, label, new Vector2(row.X, middle - (_labelFont.LineHeight / 2f)));
        page.Write(_valueFont, value.Text, new Vector2(row.Right - value.Width, middle - (_valueFont.LineHeight / 2f)), UiTheme.TextPrimary);
        page.Batch.Draw(page.Textures.Pixel, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), UiTheme.Stroke * page.Opacity);
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

        // A small label in spaced caps and the secondary color.
        public void WriteLabel(SpriteFontBase font, string text, Vector2 position) =>
            Batch.DrawString(font, text, position, UiTheme.TextSecondary * Opacity, characterSpacing: LabelSpacing);
    }

    // A section of label and value rows.
    private readonly record struct RowsSection(SectionTitle Title, IReadOnlyList<string> Labels, IReadOnlyList<MeasuredText> Values);

    // The shown sheet's values, measured once when it is shown: the name's width places the level beside it.
    private sealed record MeasuredSheet(
        float NameWidth, MeasuredText[] Parameters, MeasuredText[] Worn, string[] ResourceNames, MeasuredText[] Resources);
}
