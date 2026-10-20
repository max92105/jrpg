using System.Diagnostics.CodeAnalysis;
using FontStashSharp;
using Jrpg.Core.Content;
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
    private const int EquipmentTop = 44;
    private const int SectionHeight = 28;
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
    private const int PreviewArrowGap = 70;
    private const int PreviewArrowSide = 6;
    private const int PreviewValueGap = 16;
    private const int NoticeGap = 24;
    private const int AttackLine = 2;
    private const int DefenseLine = 3;

    private readonly PartyState _party;
    private readonly PartyEquipment _equipment;
    private readonly Inventory _inventory;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private readonly UiMessages _messages;
    private readonly EquipmentChoiceView _choiceView;
    private readonly UiTextures _ui;
    private readonly ListCursor _memberCursor;
    private readonly ListCursor _rowCursor = new(EquipmentSheet.Slots.Length + 1, _ => true);
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SectionTitle _equipmentTitle;
    private readonly SectionTitle _parametersTitle;
    private readonly EquipmentLabels _labels;
    private EquipmentSheet _sheet;
    private EquipmentChoice? _choice;
    private StatPreviewText? _preview;
    // The choice row the preview was built for; -1 when none was.
    private int _previewedRow = -1;
    private string? _notice;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, icons, the selected row.</param>
    /// <param name="session">Whose party and inventory.</param>
    /// <param name="catalog">The items, to list what each slot can take.</param>
    public EquipmentPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        _party = session.Party;
        _equipment = session.Equipment;
        _inventory = session.Inventory;
        _items = catalog.Items;
        _messages = text.Messages;
        _choiceView = new EquipmentChoiceView(text, ui);
        _ui = ui;
        _memberCursor = new ListCursor(_party.Members.Count, _ => true);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _equipmentTitle = new SectionTitle(text, ui, MessageIds.StatusEquipment);
        _parametersTitle = new SectionTitle(text, ui, MessageIds.StatusParameters);
        _labels = EquipmentLabels.From(text.Messages);
        Open();
    }

    /// <summary>Everyone recruited: the active party in its order, then the reserve.</summary>
    public IReadOnlyList<PartyMember> Members { get; private set; }

    public PartyMember Shown => Members[_memberCursor.Selected];

    public string TitleMessageId => MessageIds.MenuEquipment;

    // Recruits don't change while the menu is open, so the member cursor's count still holds.
    [MemberNotNull(nameof(Members), nameof(_sheet))]
    public void Open()
    {
        Members = MemberOrder.ActiveThenReserve(_party);
        _memberCursor.Select(0);
        _rowCursor.Select(0);
        _sheet = EquipmentSheet.Of(Shown);
        _notice = null;
    }

    /// <summary>False while choosing what goes in a slot: Cancel closes that instead.</summary>
    public bool LeavesOnCancel => _choice is null;

    /// <summary>Whether the player is choosing what goes in a slot.</summary>
    public bool IsChoosing => _choice is not null;

    /// <summary>Why the last change didn't happen, or null.</summary>
    public string? Notice => _notice;

    public SceneRequest? Update(InputFrame input)
    {
        if (_choice is not null)
        {
            UpdateChoice(_choice, input);
            return null;
        }

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
            _sheet = EquipmentSheet.Of(Shown);
            _notice = null;
            return null;
        }

        MoveOrChoose(input);
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var right = _choice is null ? area.Right : area.Right - EquipmentChoiceView.PanelWidth;
        var width = right - Margin - left;
        batch.DrawString(_headingFont, _sheet.Name, new Vector2(left, top), UiTheme.TextPrimary * opacity);

        var equipment = new Rectangle(left, top + EquipmentTop, width, 0);
        _equipmentTitle.Draw(batch, textures, equipment, opacity);
        var rowsTop = equipment.Y + SectionHeight;
        for (var index = 0; index < EquipmentSheet.Slots.Length; index++)
        {
            DrawSlotRow(batch, textures, new Rectangle(left, rowsTop + (index * RowHeight), width, RowHeight), index, opacity);
        }

        var removeAllTop = rowsTop + (EquipmentSheet.Slots.Length * RowHeight) + ActionGap;
        batch.Draw(textures.Pixel, new Rectangle(left, removeAllTop - 1, width, 1), UiTheme.Stroke * opacity);
        DrawRemoveAllRow(batch, textures, new Rectangle(left, removeAllTop, width, RowHeight), opacity);

        var parameters = new Rectangle(left, removeAllTop + RowHeight + ParametersGap, width, 0);
        DrawParameters(batch, textures, parameters, opacity);
        if (_notice is not null)
        {
            var noticeTop = parameters.Y + SectionHeight + (2 * StatLineHeight) + NoticeGap;
            batch.DrawString(_bodyFont, _notice, new Vector2(left + RowInset, noticeTop), UiTheme.Danger * opacity);
        }

        if (_choice is not null)
        {
            _choiceView.Draw(batch, textures, _choice, new Rectangle(right, area.Y, EquipmentChoiceView.PanelWidth, area.Height));
        }
    }

    /// <summary>
    /// Takes off what the member wears, slot by slot. A worn item whose inventory stack is already full stays worn, as
    /// <see cref="PartyEquipment.TryUnequip"/> refuses it.
    /// </summary>
    private void RemoveAll()
    {
        foreach (var slot in EquipmentSheet.Slots)
        {
            _equipment.TryUnequip(Shown, slot);
        }

        _sheet = EquipmentSheet.Of(Shown);
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

        if (!input.IsPressed(GameButton.Confirm))
        {
            return;
        }

        _notice = null;
        if (_rowCursor.Selected == EquipmentSheet.Slots.Length)
        {
            RemoveAll();
            return;
        }

        _choice = new EquipmentChoice(_equipment.WearableFor(Shown, EquipmentSheet.Slots[_rowCursor.Selected], _items));
        _choiceView.Show(_choice, _labels.Slots[_rowCursor.Selected], _inventory);
        ShowPreview(_choice);
    }

    private void UpdateChoice(EquipmentChoice choice, InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            CloseChoice();
            return;
        }

        if (choice.Update(input) is { } pick)
        {
            Apply(pick, EquipmentSheet.Slots[_rowCursor.Selected]);
            CloseChoice();
            return;
        }

        ShowPreview(choice);
    }

    // Rebuilt when the highlighted row changes, so drawing formats nothing; on "Remove" it previews the empty slot.
    private void ShowPreview(EquipmentChoice choice)
    {
        if (choice.SelectedRow == _previewedRow)
        {
            return;
        }

        _previewedRow = choice.SelectedRow;
        var slot = EquipmentSheet.Slots[_rowCursor.Selected];
        _preview = StatPreviewText.Of(choice.Highlighted is { } item ? StatPreview.Of(Shown, item) : StatPreview.OfRemoving(Shown, slot));
    }

    private void CloseChoice()
    {
        _choice = null;
        _preview = null;
        _previewedRow = -1;
    }

    // Core refuses a change when the item it would take off has no room left in the inventory; the player is told why.
    private void Apply(EquipmentPick pick, EquipmentSlot slot)
    {
        var changed = pick.Item is null ? _equipment.TryUnequip(Shown, slot) : _equipment.TryEquip(Shown, pick.Item);
        if (!changed && Shown.EquippedIn(slot) is { } worn && !_inventory.HasRoomFor(worn))
        {
            _notice = _messages.Format(MessageIds.EquipmentNoRoom, new MessageArgument("item", worn.Name));
        }

        _sheet = EquipmentSheet.Of(Shown);
    }

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
        if (_rowCursor.Selected == EquipmentSheet.Slots.Length)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity);
        }

        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, _labels.RemoveAll, new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.TextPrimary * opacity);
    }

    // Legacy StatsPanel: Max HP and Max MP, then Attack and Defense, two to a line.
    private void DrawParameters(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        _parametersTitle.Draw(batch, textures, section, opacity);
        var columnWidth = (section.Width - RowInset - ColumnGap) / 2;
        for (var index = 0; index < _labels.Stats.Length; index++)
        {
            var columnLeft = section.X + RowInset + (index % 2 * (columnWidth + ColumnGap));
            var position = new Vector2(columnLeft, section.Y + SectionHeight + (index / 2 * StatLineHeight));
            batch.DrawString(_bodyFont, _labels.Stats[index], position, UiTheme.TextSecondary * opacity);
            batch.DrawString(_bodyFont, _sheet.Stats[index], position + new Vector2(ValueOffset, 0), UiTheme.TextPrimary * opacity);
            if (_preview is not null && index is AttackLine or DefenseLine)
            {
                var value = index == AttackLine ? _preview.Attack : _preview.Defense;
                DrawPreview(batch, textures, position + new Vector2(ValueOffset + PreviewArrowGap, 0), value);
            }
        }
    }

    // Legacy StatLine's preview: an arrow, drawn as a small diamond since the font lacks ▸, then the new value colored by
    // whether it goes up or down. The choice only opens once the menu has faded in, so it draws at full opacity.
    private void DrawPreview(SpriteBatch batch, TextureLibrary textures, Vector2 position, StatPreviewText.PreviewValue value)
    {
        var arrowCenter = position + new Vector2(PreviewArrowSide, _bodyFont.LineHeight / 2f);
        new Diamond(arrowCenter, PreviewArrowSide).Draw(batch, textures.Pixel, UiTheme.TextSecondary);
        batch.DrawString(_bodyFont, value.Text, position + new Vector2(PreviewValueGap, 0), value.Color);
    }

    // The page's fixed text; Slots follows EquipmentSheet.Slots, Stats the sheet's.
    private sealed record EquipmentLabels(string[] Slots, string Empty, string RemoveAll, string[] Stats)
    {
        public static EquipmentLabels From(UiMessages messages) => new(
            [.. EquipmentSheet.Slots.Select(slot => messages.Text(EquipmentSheet.LabelIdOf(slot)))],
            messages.Text(MessageIds.SlotEmpty),
            messages.Text(MessageIds.EquipmentRemoveAll),
            [messages.Text(MessageIds.StatMaxHp), messages.Text(MessageIds.StatMaxMp), messages.Text(MessageIds.StatAttack), messages.Text(MessageIds.StatDefense)]);
    }
}
