using System.Collections.Immutable;
using System.Globalization;
using FontStashSharp;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's Equipment page (legacy <c>EquipmentView</c>): one member at a time, what they wear in each slot, a "Remove
/// all" action, and their parameters. Changing what's worn comes in P3b.
/// </summary>
internal sealed class EquipmentPage : IMenuPage
{
    // Legacy EquipmentView: a 28 x 20 margin, the member's name as Text.Heading, section titles over a hairline, 44 px
    // ListRows (a 26 px icon in a 40 px column, the slot name, then what's worn), a hairline 10 px above "Remove all",
    // and the parameters as two columns of 36 px lines 40 px apart.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int LabelPixels = 14;
    private const int EquipmentTop = 44;
    private const int SectionHeight = 28;
    private const int SectionLineGap = 20;
    private const int RowHeight = 44;
    private const int RowInset = 10;
    private const int IconColumn = 40;
    private const int IconSize = 26;
    private const int WornColumn = 200;
    private const int ActionGap = 10;
    private const int ParametersGap = 18;
    private const int StatLineHeight = 36;
    private const int ColumnGap = 40;
    private const int ValueOffset = 160;

    // The rows: one per slot, in this order, then "Remove all".
    private static readonly ImmutableArray<EquipmentSlot> _slots = [EquipmentSlot.Weapon, EquipmentSlot.Armor];

    private readonly PartyEquipment _equipment;
    private readonly UiTextures _ui;
    private readonly ListCursor _memberCursor;
    private readonly ListCursor _rowCursor = new(_slots.Length + 1, _ => true);
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _labelFont;
    private readonly EquipmentLabels _labels;
    private MemberSheet _sheet;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, icons, the selected row.</param>
    /// <param name="session">Whose party and inventory.</param>
    public EquipmentPage(ScreenText text, UiTextures ui, GameSession session)
    {
        _equipment = session.Equipment;
        _ui = ui;
        Members = MemberOrder.ActiveThenReserve(session.Party);
        _memberCursor = new ListCursor(Members.Count, _ => true);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _labels = EquipmentLabels.From(text.Messages);
        _sheet = SheetOf(Shown);
    }

    /// <summary>Everyone recruited: the active party in its order, then the reserve.</summary>
    public IReadOnlyList<PartyMember> Members { get; }

    public PartyMember Shown => Members[_memberCursor.Selected];

    public string TitleMessageId => MessageIds.MenuEquipment;

    public bool LeavesOnCancel => true;

    public SceneRequest? Update(InputFrame input)
    {
        var shownBefore = _memberCursor.Selected;
        if (input.IsPressed(GameButton.PreviousTab) || input.IsPressed(GameButton.Left))
        {
            _memberCursor.MoveUp();
        }

        if (input.IsPressed(GameButton.NextTab) || input.IsPressed(GameButton.Right))
        {
            _memberCursor.MoveDown();
        }

        if (_memberCursor.Selected != shownBefore)
        {
            _sheet = SheetOf(Shown);
            return null;
        }

        MoveOrChoose(input);
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var width = area.Right - Margin - left;
        batch.DrawString(_headingFont, _sheet.Name, new Vector2(left, top), UiTheme.TextPrimary * opacity);

        var equipment = new Rectangle(left, top + EquipmentTop, width, 0);
        DrawSectionTitle(batch, textures, _labels.Equipment, equipment, opacity);
        var rowsTop = equipment.Y + SectionHeight;
        for (var index = 0; index < _slots.Length; index++)
        {
            DrawSlotRow(batch, textures, new Rectangle(left, rowsTop + (index * RowHeight), width, RowHeight), index, opacity);
        }

        var removeAllTop = rowsTop + (_slots.Length * RowHeight) + ActionGap;
        batch.Draw(textures.Pixel, new Rectangle(left, removeAllTop - 1, width, 1), UiTheme.Stroke * opacity);
        DrawRemoveAllRow(batch, textures, new Rectangle(left, removeAllTop, width, RowHeight), opacity);

        var parameters = new Rectangle(left, removeAllTop + RowHeight + ParametersGap, width, 0);
        DrawParameters(batch, textures, parameters, opacity);
    }

    /// <summary>
    /// Takes off what the member wears, slot by slot. A worn item whose inventory stack is already full stays worn, as
    /// <see cref="PartyEquipment.TryUnequip"/> refuses it.
    /// </summary>
    private void RemoveAll()
    {
        foreach (var slot in _slots)
        {
            _equipment.TryUnequip(Shown, slot);
        }

        _sheet = SheetOf(Shown);
    }

    private void MoveOrChoose(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _rowCursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _rowCursor.MoveDown();
        }

        if (input.IsPressed(GameButton.Confirm) && _rowCursor.Selected == _slots.Length)
        {
            RemoveAll();
        }
    }

    // Built when the member or what they wear changes, so drawing formats nothing.
    private static MemberSheet SheetOf(PartyMember member)
    {
        var stats = member.TotalStats;
        return new MemberSheet(
            member.Definition.Name,
            [.. _slots.Select(slot => WornIn(member, slot))],
            [Number(member.Vitals.MaxHp), Number(member.Vitals.MaxMp), Number(stats.Attack), Number(stats.Defense)]);
    }

    private static ItemDefinition? WornIn(PartyMember member, EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => member.Weapon,
        EquipmentSlot.Armor => member.Armor,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
    };

    private static string LabelIdOf(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => MessageIds.SlotWeapon,
        EquipmentSlot.Armor => MessageIds.SlotArmor,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
    };

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private void DrawSlotRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, int index, float opacity)
    {
        if (index == _rowCursor.Selected)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity);
        }

        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, _labels.Slots[index], new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.TextSecondary * opacity);
        var wornLeft = row.X + RowInset + IconColumn + WornColumn;
        if (_sheet.Worn[index] is not { } worn)
        {
            batch.DrawString(_bodyFont, _labels.Empty, new Vector2(wornLeft, textTop), UiTheme.TextDisabled * opacity);
            return;
        }

        var iconTop = row.Y + ((RowHeight - IconSize) / 2);
        batch.Draw(_ui.Icon(textures, worn.IconFile, IconSize), new Vector2(wornLeft - IconColumn, iconTop), Color.White * opacity);
        batch.DrawString(_bodyFont, worn.Name, new Vector2(wornLeft, textTop), UiTheme.TextPrimary * opacity);
    }

    private void DrawRemoveAllRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, float opacity)
    {
        if (_rowCursor.Selected == _slots.Length)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity);
        }

        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, _labels.RemoveAll, new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.TextPrimary * opacity);
    }

    // Legacy StatsPanel: Max HP and Max MP, then Attack and Defense, two to a line.
    private void DrawParameters(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        DrawSectionTitle(batch, textures, _labels.Parameters, section, opacity);
        var columnWidth = (section.Width - RowInset - ColumnGap) / 2;
        for (var index = 0; index < _labels.Stats.Length; index++)
        {
            var columnLeft = section.X + RowInset + (index % 2 * (columnWidth + ColumnGap));
            var position = new Vector2(columnLeft, section.Y + SectionHeight + (index / 2 * StatLineHeight));
            batch.DrawString(_bodyFont, _labels.Stats[index], position, UiTheme.TextSecondary * opacity);
            batch.DrawString(_bodyFont, _sheet.Stats[index], position + new Vector2(ValueOffset, 0), UiTheme.TextPrimary * opacity);
        }
    }

    // Legacy SectionHeaderStyle: a small label over a hairline that fades out to the right.
    private void DrawSectionTitle(SpriteBatch batch, TextureLibrary textures, string title, Rectangle section, float opacity)
    {
        batch.DrawString(_labelFont, title, new Vector2(section.X, section.Y), UiTheme.TextSecondary * opacity);
        var line = new Rectangle(section.X, section.Y + SectionLineGap, section.Width, 1);
        batch.Draw(_ui.SectionLine(textures), line, Color.White * opacity);
    }

    // The shown member: what they wear in _slots' order (null for nothing) and their parameter values, in the labels' order.
    private sealed record MemberSheet(string Name, ItemDefinition?[] Worn, string[] Stats);

    // The page's fixed text; Slots follows _slots, Stats the parameters' order.
    private sealed record EquipmentLabels(string Equipment, string[] Slots, string Empty, string RemoveAll, string Parameters, string[] Stats)
    {
        public static EquipmentLabels From(UiMessages messages) => new(
            messages.Text(MessageIds.StatusEquipment),
            [.. _slots.Select(slot => messages.Text(LabelIdOf(slot)))],
            messages.Text(MessageIds.SlotEmpty),
            messages.Text(MessageIds.EquipmentRemoveAll),
            messages.Text(MessageIds.StatusParameters),
            [messages.Text(MessageIds.StatMaxHp), messages.Text(MessageIds.StatMaxMp), messages.Text(MessageIds.StatAttack), messages.Text(MessageIds.StatDefense)]);
    }
}
