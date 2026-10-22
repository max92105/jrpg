using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's Status page: one party member at a time, with their profile and experience beside a diamond-framed
/// portrait, then their parameters and what they wear. Left and Right (or the tab buttons) go through the members,
/// active ones first.
/// </summary>
internal sealed class StatusPage : IMenuPage
{
    // Placeholder layout (decision 82): a 28 x 20 margin, 22 px headings, 18 px body text, 14 px labels and captions;
    // a 190 px portrait 32 px right of the profile; 36 px stat lines in two columns 40 px apart, 10 px in under titles.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int LabelPixels = 14;
    private const int PortraitSize = 190;
    private const int PortraitFrame = 2;
    private const int PortraitGap = 32;
    private const int Indent = 10;
    private const int StatLineHeight = 36;
    private const int ColumnGap = 40;
    private const int ValueOffset = 160;

    // Rows, down from the page's top margin: the profile, then each section's title and its lines.
    private const int ClassTop = 32;
    private const int ExperienceTop = 64;
    private const int ParametersTop = PortraitSize + 18;
    private const int EquipmentTop = ParametersTop + SectionHeight + (2 * StatLineHeight) + 18;
    private const int SectionHeight = 28;
    private const int ValueBelowLabel = 20;

    // The experience bar: 6 px tall, 12 px under the experience values, 16 px short of the profile's right edge. HP and
    // MP get the same slim bar under their lines.
    private const int BarHeight = 6;
    private const int XpBarTop = ValueBelowLabel + 34;
    private const int XpBarRightGap = 16;
    private const int VitalBarTop = 26;

    private readonly ContentCatalog _catalog;
    private readonly UiTextures _ui;
    private readonly PartyState _party;
    private readonly ListCursor _cursor;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _labelFont;
    private readonly StatusLabels _labels;
    private readonly SectionTitle _experienceTitle;
    private readonly SectionTitle _parametersTitle;
    private readonly SectionTitle _equipmentTitle;
    private readonly MemberVitalBars _vitals;
    private readonly StatBar _xpBar;
    private MemberSheet _sheet;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines and baked portraits.</param>
    /// <param name="session">Whose party is shown.</param>
    /// <param name="catalog">Class names and the experience table.</param>
    public StatusPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        Hint = text.Messages.Text(MessageIds.MenuHintStatus);
        _catalog = catalog;
        _ui = ui;
        _party = session.Party;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _labels = StatusLabels.From(text.Messages);
        _experienceTitle = new SectionTitle(text, ui, MessageIds.StatusExperience);
        _parametersTitle = new SectionTitle(text, ui, MessageIds.StatusParameters);
        _equipmentTitle = new SectionTitle(text, ui, MessageIds.StatusEquipment);
        _vitals = new MemberVitalBars(text, ui, BodyPixels);
        _xpBar = new StatBar(ui, UiTheme.XpBar);
        _cursor = new ListCursor(_party.Members.Count, _ => true);
        Open();
    }

    /// <summary>Everyone recruited: the active party in its order, then the reserve.</summary>
    public IReadOnlyList<PartyMember> Members { get; private set; }

    public PartyMember Shown => Members[_cursor.Selected];

    public string TitleMessageId => MessageIds.MenuStatus;

    public string Hint { get; }

    public bool ShowsPartySidebar => true;

    public bool LeavesOnCancel => true;

    /// <summary>The shown member's HP, MP, Attack and Defense, as the page writes them.</summary>
    internal IReadOnlyList<string> ShownStats => _sheet.Stats;

    // Recruits don't change while the menu is open, so the cursor's member count still holds.
    [MemberNotNull(nameof(Members), nameof(_sheet))]
    public void Open()
    {
        Members = MemberOrder.ActiveThenReserve(_party);
        _cursor.Select(0);
        ShowMember();
    }

    public SceneRequest? Update(InputFrame input)
    {
        var before = _cursor.Selected;
        if (input.IsPressed(GameButton.Left) || input.IsPressed(GameButton.PreviousTab))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Right) || input.IsPressed(GameButton.NextTab))
        {
            _cursor.MoveDown();
        }

        if (_cursor.Selected != before)
        {
            ShowMember();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var portraitLeft = area.Right - Margin - PortraitSize;
        var profileWidth = portraitLeft - PortraitGap - left;
        var fullWidth = area.Right - Margin - left;
        var page = new PageFrame(batch, textures, opacity);

        DrawPortrait(page, new Vector2(portraitLeft + (PortraitSize / 2f), top + (PortraitSize / 2f)));
        page.Write(_headingFont, _sheet.Name, new Vector2(left, top), UiTheme.TextPrimary);
        page.Write(_labelFont, _sheet.ClassName, new Vector2(left, top + ClassTop), UiTheme.TextSecondary);
        DrawExperience(page, new Rectangle(left, top + ExperienceTop, profileWidth, 0));
        DrawParameters(page, new Rectangle(left, top + ParametersTop, fullWidth, 0));
        DrawEquipment(page, new Rectangle(left, top + EquipmentTop, fullWidth, 0));
    }

    // The text and bars are set when the member changes, so drawing only reads them.
    [MemberNotNull(nameof(_sheet))]
    private void ShowMember()
    {
        var member = Shown;
        _vitals.Show(member.Vitals);
        _sheet = SheetOf(member, _vitals);
        var (earned, needed) = ExperienceProgress(member.Level, member.Experience, _catalog.Rules.Experience);
        _xpBar.Show(earned, needed);
    }

    // The HP and MP texts come from the meters, already showing the member.
    private MemberSheet SheetOf(PartyMember member, MemberVitalBars shownVitals)
    {
        var nextLevel = ToNextLevelText(member.Level, member.Experience, _catalog.Rules.Experience, _labels.MaxLevel);
        var stats = member.TotalStats;
        return new MemberSheet(
            member.Definition.Name,
            _catalog.Classes[member.Definition.ClassId].Name,
            member.Definition.PortraitFile,
            [Number(member.Level), Number(member.Experience), nextLevel],
            [
                shownVitals.Hp.Value.Text,
                shownVitals.Mp.Value.Text,
                Number(stats.Attack),
                Number(stats.Defense),
            ],
            [member.Weapon?.Name ?? _labels.Empty, member.Armor?.Name ?? _labels.Empty]);
    }

    /// <summary>The experience still needed for the next level, or <paramref name="maxLevelText"/> at the highest level.</summary>
    /// <param name="level">The member's level.</param>
    /// <param name="experience">Earned toward the next level, as <see cref="PartyMember.Experience"/> counts it.</param>
    /// <param name="table">How much each level takes.</param>
    /// <param name="maxLevelText">Shown when there is no next level.</param>
    internal static string ToNextLevelText(int level, int experience, ExperienceTable table, string maxLevelText) =>
        table.IsMaxLevel(level) ? maxLevelText : Number(table.ExperienceToReachNext(level) - experience);

    /// <summary>How far the member is through their level, for the experience bar; all the way at the highest level.</summary>
    /// <param name="level">The member's level.</param>
    /// <param name="experience">Earned toward the next level.</param>
    /// <param name="table">How much each level takes.</param>
    internal static (int Earned, int Needed) ExperienceProgress(int level, int experience, ExperienceTable table) =>
        table.IsMaxLevel(level) ? (1, 1) : (experience, table.ExperienceToReachNext(level));

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    // A diamond like the dialogue's, framed by a slightly larger pale diamond behind it.
    private void DrawPortrait(PageFrame page, Vector2 center)
    {
        Diamond.WithDiagonal(center, PortraitSize + (2 * PortraitFrame)).Draw(page.Batch, page.Textures.Pixel, UiTheme.BarLine * page.Opacity);
        var portrait = _ui.DiamondPortrait(page.Textures, _sheet.PortraitFile, PortraitSize);
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
            page.Write(_headingFont, _sheet.Progress[column], new Vector2(columnLeft, labelsTop + ValueBelowLabel), UiTheme.TextPrimary);
        }

        var xpBar = new Rectangle(section.X + Indent, labelsTop + XpBarTop, section.Width - Indent - XpBarRightGap, BarHeight);
        _xpBar.Draw(page.Batch, page.Textures, xpBar, page.Opacity);
    }

    // HP and MP on the first line, attack and defense on the second, in two columns.
    private void DrawParameters(PageFrame page, Rectangle section)
    {
        _parametersTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        var columnWidth = (section.Width - Indent - ColumnGap) / 2;
        for (var index = 0; index < _labels.Stats.Length; index++)
        {
            var columnLeft = section.X + Indent + (index % 2 * (columnWidth + ColumnGap));
            var lineTop = section.Y + SectionHeight + (index / 2 * StatLineHeight);
            DrawLine(page, new Vector2(columnLeft, lineTop), _labels.Stats[index], _sheet.Stats[index]);
        }

        // HP and MP are the first line, so their bars go under its two columns.
        var hpBar = new Rectangle(section.X + Indent, section.Y + SectionHeight + VitalBarTop, columnWidth, BarHeight);
        _vitals.Hp.Bar.Draw(page.Batch, page.Textures, hpBar, page.Opacity);
        var mpBar = hpBar with { X = hpBar.X + columnWidth + ColumnGap };
        _vitals.Mp.Bar.Draw(page.Batch, page.Textures, mpBar, page.Opacity);
    }

    private void DrawEquipment(PageFrame page, Rectangle section)
    {
        _equipmentTitle.Draw(page.Batch, page.Textures, section, page.Opacity);
        for (var index = 0; index < _labels.Slots.Length; index++)
        {
            var lineTop = section.Y + SectionHeight + (index * StatLineHeight);
            DrawLine(page, new Vector2(section.X + Indent, lineTop), _labels.Slots[index], _sheet.Worn[index]);
        }
    }

    private void DrawLine(PageFrame page, Vector2 position, string label, string value)
    {
        page.Write(_bodyFont, label, position, UiTheme.TextSecondary);
        page.Write(_bodyFont, value, position + new Vector2(ValueOffset, 0), UiTheme.TextPrimary);
    }

    // What the page draws with this frame, so the drawing helpers stay at a few parameters.
    private readonly record struct PageFrame(SpriteBatch Batch, TextureLibrary Textures, float Opacity)
    {
        public void Write(SpriteFontBase font, string text, Vector2 position, Color color) =>
            Batch.DrawString(font, text, position, color * Opacity);
    }

    // The shown member's text, built when the member changes rather than every frame.
    private sealed record MemberSheet(
        string Name,
        string ClassName,
        string PortraitFile,
        string[] Progress,
        string[] Stats,
        string[] Worn);

    // The page's fixed text, in the order the sheet's arrays use.
    private sealed record StatusLabels(
        string[] Progress,
        string MaxLevel,
        string[] Stats,
        string[] Slots,
        string Empty)
    {
        public static StatusLabels From(UiMessages messages) => new(
            [messages.Text(MessageIds.StatusLevel), messages.Text(MessageIds.StatusExp), messages.Text(MessageIds.StatusNextLevel)],
            messages.Text(MessageIds.StatusMaxLevel),
            [messages.Text(MessageIds.StatHp), messages.Text(MessageIds.StatMp), messages.Text(MessageIds.StatAttack), messages.Text(MessageIds.StatDefense)],
            [messages.Text(MessageIds.SlotWeapon), messages.Text(MessageIds.SlotArmor)],
            messages.Text(MessageIds.SlotEmpty));
    }
}
