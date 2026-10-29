using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws a <see cref="StatusSheet"/> after the reference's Status page (decision 334): the profile beside a diamond
/// portrait, with the experience over its bar and the experience to the next level at the bar's end, and the five
/// Attributes with what each gives at the band's right; then each section under its dark band
/// (<see cref="SectionTitle"/>): Combat beside Resistances (<see cref="StatusParametersView"/>), then the skills with
/// Resources at their right (<see cref="StatusResourcesView"/>, decision 368).
/// </summary>
internal sealed class StatusSheetView
{
    // Placeholder layout (decisions 82, 334): a 28 x 20 margin; a 160 px portrait 32 px left of the profile's text: a
    // 36 px name with the level 24 px after it, a 20 px class 48 px under the name's top, the experience row 92 px
    // under the name's top; 18 px labels 2 px apart, 26 px values.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int NamePixels = 36;
    private const int ClassPixels = 20;
    /// <summary>The page's small spaced-caps labels' size, shared with <see cref="StatusParametersView"/>.</summary>
    internal const int LabelPixels = 18;

    /// <summary>The page's large values' size, shared with <see cref="StatusParametersView"/>.</summary>
    internal const int ValuePixels = 26;

    /// <summary>The labels' letter spacing, shared with <see cref="StatusParametersView"/>.</summary>
    internal const float LabelSpacing = 2;

    private const int PortraitSize = 160;
    private const int PortraitFrame = 2;
    private const int PortraitGap = 32;
    private const int LevelGap = 24;
    private const int ClassTop = 48;
    private const int ExperienceTop = 92;
    private const int LabelValueGap = 12;

    // The experience bar, 6 px tall, 40 px under the experience row's top, across the profile's text.
    private const int BarHeight = 6;
    private const int XpBarTop = 40;

    // Under the profile band (as tall as Attributes) and 16 px, the middle band, 16 px, then Skills: room for three
    // skills with two-line descriptions (decision 373; the skills view draws as many whole ones as fit, and the layout
    // test measures every class's). Resources sit 48 px right of Skills, 300 px wide (decision 368).
    private const int ProfileHeight = AttributesHeight + SectionGap;
    private const int SectionGap = 16;
    private const int ResourcesWidth = 300;
    private const int ResourcesGap = 48;

    /// <summary>The room the page keeps for the Skills row, which the Resources beside them must fit.</summary>
    internal const int SkillsHeight = StatusSkillsView.RoomForThreeSkills;

    // The Attributes column at the profile band's right (S1b): 480 px wide, 40 px from the profile's text, five 34 px
    // rows 10 px under its band, each label 12 px in, its value ending 190 px in, the caption 16 px after it in 16 px
    // type. Its height sets the profile band's: 46 + 5 x 34 = 216, taller than the 160 px portrait.
    private const int AttributesWidth = 480;
    private const int AttributesGap = 40;
    private const int AttributeRowHeight = 34;
    private const int AttributesBelowBand = SectionTitle.Height + 10;
    private const int AttributesHeight = AttributesBelowBand + (5 * AttributeRowHeight);
    private const int AttributeInset = 12;
    private const int AttributeValueRight = 190;
    private const int CaptionGap = 16;
    private const int CaptionPixels = 16;

    /// <summary>
    /// How tall the page's content is, from the area's top: what the layout test checks against the page area's height.
    /// </summary>
    internal const int ContentHeight = TopMargin + ProfileHeight + StatusParametersView.Height + SectionGap + SkillsHeight;

    // The level, the experience and the experience to the next level, in the order of both Progress arrays.
    private const int LevelIndex = 0;
    private const int ExperienceIndex = 1;
    private const int NextIndex = 2;

    private readonly UiTextures _ui;
    private readonly StatusLabels _labels;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _classFont;
    private readonly SpriteFontBase _labelFont;
    private readonly SpriteFontBase _valueFont;
    private readonly float[] _progressLabelWidths;
    private readonly StatusParametersView _parameters;
    private readonly SectionTitle _attributesTitle;
    private readonly SpriteFontBase _captionFont;
    private readonly StatusSkillsView _skills;
    private readonly StatusResourcesView _resources;
    private readonly StatBar _xpBar;
    private MeasuredSheet? _shown;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: bars and baked portraits.</param>
    /// <param name="labels">The captions, in the sheet's order.</param>
    public StatusSheetView(ScreenText text, UiTextures ui, StatusLabels labels)
    {
        _ui = ui;
        _labels = labels;
        _nameFont = text.Fonts.FontAt(NamePixels);
        _classFont = text.Fonts.FontAt(ClassPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _valueFont = text.Fonts.FontAt(ValuePixels);
        _progressLabelWidths = [.. labels.Progress.Select(label => _labelFont.MeasureString(label, characterSpacing: LabelSpacing).X)];
        _parameters = new StatusParametersView(text, labels);
        _attributesTitle = new SectionTitle(text, MessageIds.StatusAttributes);
        _captionFont = text.Fonts.FontAt(CaptionPixels);
        _skills = new StatusSkillsView(text);
        _resources = new StatusResourcesView(text);
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
            MeasuredText.Of(_valueFont, sheet.Progress[NextIndex]),
            [.. sheet.Attributes.Select(attribute => MeasuredText.Of(_valueFont, attribute.Value))]);
        _parameters.Show(sheet);
        _resources.Show(sheet);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel, bars and portraits come from.</param>
    /// <param name="place">The sheet, and the page's area.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    /// <exception cref="InvalidOperationException">No sheet was shown yet.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var shown = _shown ?? throw new InvalidOperationException("Show a sheet before drawing it.");
        var area = place.Area;
        var content = new Rectangle(area.X + Margin, area.Y + TopMargin, area.Width - (2 * Margin), area.Height - TopMargin);
        var page = new PageFrame(batch, textures, opacity, place.Sheet);
        DrawProfile(page, content, shown);

        var band = new Rectangle(content.X, content.Y + ProfileHeight, content.Width, StatusParametersView.Height);
        _parameters.Draw(batch, textures, new StatusParametersView.Place(band, Vitals.Hp.Bar), opacity);
        var skills = SkillsSectionIn(area);
        _skills.Draw(batch, textures, new StatusSkillsView.Place(place.Sheet, skills), opacity);
        var resources = new Rectangle(skills.Right + ResourcesGap, skills.Y, ResourcesWidth, StatusResourcesView.Height);
        _resources.Draw(batch, textures, resources, opacity);
    }

    /// <summary>The Skills section: under the middle band down to the page's bottom, left of Resources.</summary>
    /// <param name="area">The page's area.</param>
    internal static Rectangle SkillsSectionIn(Rectangle area)
    {
        var top = area.Y + TopMargin + ProfileHeight + StatusParametersView.Height + SectionGap;
        var width = area.Width - (2 * Margin) - ResourcesGap - ResourcesWidth;
        return new Rectangle(area.X + Margin, top, width, area.Bottom - top);
    }

    // The portrait at the left; name and level, the class under them, the experience over its bar; Attributes right.
    private void DrawProfile(PageFrame page, Rectangle content, MeasuredSheet shown)
    {
        DrawPortrait(page, new Vector2(content.X + (PortraitSize / 2f), content.Y + (PortraitSize / 2f)));
        var textLeft = content.X + PortraitSize + PortraitGap;
        page.Write(_nameFont, page.Sheet.Name, new Vector2(textLeft, content.Y), UiTheme.TextPrimary);
        var levelLeft = textLeft + shown.NameWidth + LevelGap;
        var levelTop = content.Y + ((_nameFont.LineHeight - _valueFont.LineHeight) / 2f);
        DrawLabeledValue(page, new Vector2(levelLeft, levelTop), LevelIndex);
        page.Write(_classFont, page.Sheet.ClassName, new Vector2(textLeft, content.Y + ClassTop), UiTheme.TextSecondary);
        var attributes = new Rectangle(content.Right - AttributesWidth, content.Y, AttributesWidth, AttributesHeight);
        DrawExperience(page, new Rectangle(textLeft, content.Y + ExperienceTop, attributes.X - AttributesGap - textLeft, 0), shown.Next);
        DrawAttributes(page, attributes, shown.Attributes);
    }

    // Each primary's label, its value close to it, then what it gives in the secondary color.
    private void DrawAttributes(PageFrame page, Rectangle section, MeasuredText[] values)
    {
        _attributesTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        for (var index = 0; index < values.Length; index++)
        {
            var left = section.X + AttributeInset;
            var middle = section.Y + AttributesBelowBand + (index * AttributeRowHeight) + (AttributeRowHeight / 2f);
            page.WriteLabel(_labelFont, _labels.Attributes[index], new Vector2(left, middle - (_labelFont.LineHeight / 2f)));
            var valueLeft = left + AttributeValueRight - values[index].Width;
            page.Write(_valueFont, values[index].Text, new Vector2(valueLeft, middle - (_valueFont.LineHeight / 2f)), UiTheme.TextPrimary);
            var captionTop = middle - (_captionFont.LineHeight / 2f);
            page.Write(_captionFont, page.Sheet.Attributes[index].Caption, new Vector2(left + AttributeValueRight + CaptionGap, captionTop), UiTheme.TextSecondary);
        }
    }

    // A diamond like the dialogue's, framed by a slightly larger pale diamond behind it.
    private void DrawPortrait(PageFrame page, Vector2 center)
    {
        Diamond.WithDiagonal(center, PortraitSize + (2 * PortraitFrame)).Draw(page.Batch, page.Textures.Pixel, UiTheme.BarLine * page.Opacity);
        var portrait = _ui.DiamondPortrait(page.Textures, page.Sheet.PortraitFile, PortraitSize);
        var corner = new Vector2(center.X - (PortraitSize / 2f), center.Y - (PortraitSize / 2f));
        page.Batch.Draw(portrait, corner, Color.White * page.Opacity);
    }

    // The experience earned at the bar's start and the experience to the next level at its end, over the bar.
    private void DrawExperience(PageFrame page, Rectangle row, MeasuredText next)
    {
        DrawLabeledValue(page, new Vector2(row.X, row.Y), ExperienceIndex);
        var nextLeft = row.Right - next.Width;
        var nextLabelLeft = nextLeft - LabelValueGap - _progressLabelWidths[NextIndex];
        page.WriteLabel(_labelFont, _labels.Progress[NextIndex], new Vector2(nextLabelLeft, LabelTopOn(row.Y)));
        page.Write(_valueFont, next.Text, new Vector2(nextLeft, row.Y), UiTheme.TextPrimary);
        _xpBar.Draw(page.Batch, page.Textures, new Rectangle(row.X, row.Y + XpBarTop, row.Width, BarHeight), page.Opacity);
    }

    // A progress caption ("LV", "EXP") small, then its value large right after it, on the value's line.
    private void DrawLabeledValue(PageFrame page, Vector2 position, int progressIndex)
    {
        page.WriteLabel(_labelFont, _labels.Progress[progressIndex], new Vector2(position.X, LabelTopOn(position.Y)));
        var valueLeft = position.X + _progressLabelWidths[progressIndex] + LabelValueGap;
        page.Write(_valueFont, page.Sheet.Progress[progressIndex], new Vector2(valueLeft, position.Y), UiTheme.TextPrimary);
    }

    // A label's top so it sits centered on the value line starting at valueTop.
    private float LabelTopOn(float valueTop) => valueTop + ((_valueFont.LineHeight - _labelFont.LineHeight) / 2f);

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

    // The shown sheet's values, measured once when it is shown: the name's width places the level beside it, the
    // experience to the next level is right-aligned at the bar's end.
    private sealed record MeasuredSheet(float NameWidth, MeasuredText Next, MeasuredText[] Attributes);
}
