using System.Diagnostics.CodeAnalysis;
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
/// The menu's Equipment page: one member at a time, what they wear in each slot, a "Remove all" action, and the Changes
/// an item would make (decision 337). Confirm on a slot opens the items it can take in a column beside the sheet.
/// </summary>
internal sealed class EquipmentPage : IMemberPage
{
    private readonly PartyState _party;
    private readonly PartyEquipment _equipment;
    private readonly Inventory _inventory;
    private readonly UiMessages _messages;
    private readonly string _pageHint;
    private readonly EquipmentSheetView _sheetView;
    private readonly EquipmentChoiceView _choiceView;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private readonly ListCursor _memberCursor;
    private readonly ListCursor _rowCursor = new(EquipmentSheet.Slots.Length + 1, _ => true);
    private readonly SelectionMotion _selection = new();
    private EquipmentSheet _sheet;
    private readonly EquipmentComparison _comparison;
    private readonly SlotChoice _slotChoice;
    private string? _notice;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, icons, the selected row.</param>
    /// <param name="session">Whose party and inventory.</param>
    public EquipmentPage(ScreenText text, UiTextures ui, GameSession session)
    {
        _pageHint = text.Messages.Text(MessageIds.MenuHintEquipment);
        _party = session.Party;
        _equipment = session.Equipment;
        _inventory = session.Inventory;
        _messages = text.Messages;
        _sheetView = new EquipmentSheetView(text, ui);
        _choiceView = new EquipmentChoiceView(text, ui);
        _comparison = new EquipmentComparison(text.Messages);
        _slotChoice = new SlotChoice(session, _comparison, text.Messages);
        _memberCursor = new ListCursor(_party.Members.Count, _ => true);
        Open();
    }

    /// <summary>Everyone recruited: the active party in its order, then the reserve.</summary>
    public IReadOnlyList<PartyMember> Members { get; private set; }

    public PartyMember Shown => Members[_memberCursor.Selected];

    public bool HasOwnPointer => true;

    public string TitleMessageId => MessageIds.MenuEquipment;

    public TabGlyph Glyph => TabGlyph.Equipment;

    public string Hint => _slotChoice.TakeHint ?? _pageHint;

    // Recruits don't change while the menu is open, so the member cursor's count still holds.
    [MemberNotNull(nameof(Members), nameof(_sheet))]
    public void Open()
    {
        Members = MemberOrder.ActiveThenReserve(_party);
        _memberCursor.Select(0);
        _rowCursor.Select(0);
        _selection.JumpTo(_rowCursor);
        _sheet = EquipmentSheet.Of(Shown);
        _notice = null;
        ShowWornComparison();
    }

    /// <summary>What the Changes section compares; null when it shows nothing.</summary>
    internal StatComparison? Comparison => _comparison.Shown;

    /// <summary>Where the sheet's highlight is, in rows.</summary>
    internal float SelectionPosition => _selection.Position;

    public void Advance(TimeSpan delta)
    {
        _selection.Advance(delta);
        _slotChoice.Choice?.Selection.Advance(delta);
    }

    /// <summary>False while choosing what goes in a slot: Cancel closes that instead.</summary>
    public bool LeavesOnCancel => !_slotChoice.IsOpen;

    /// <summary>Whether the player is choosing what goes in a slot.</summary>
    public bool IsChoosing => _slotChoice.IsOpen;

    /// <summary>The open choice, so tests can pick a row by what it holds; null while no slot is being chosen for.</summary>
    internal EquipmentChoice? Choice => _slotChoice.Choice;

    /// <summary>Why the last change didn't happen, or null.</summary>
    public string? Notice => _notice;

    /// <summary>
    /// The comparison band's caption: "Changes" while a choice is open (what the highlighted item would change),
    /// "Gives" otherwise (what the highlighted worn item gives).
    /// </summary>
    public string ChangesCaptionId => _slotChoice.IsOpen ? MessageIds.EquipmentChanges : MessageIds.EquipmentGives;

    public SceneRequest? Update(InputFrame input)
    {
        if (_slotChoice.IsOpen)
        {
            UpdateChoice(input);
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
            ShowWornComparison();
            return null;
        }

        MoveOrChoose(input);
        return null;
    }

    // The sheet's column keeps its width whether or not the choice shows (decision 290): while choosing, the choice
    // column beside it closes the page, otherwise the sheet closes it.
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var sheetArea = area with { Width = area.Width - EquipmentChoiceView.PanelWidth };
        _panel.Draw(batch, textures.Pixel, MenuLayout.FullHeight(sheetArea), opacity);
        _sheetView.Draw(batch, textures, new EquipmentSheetView.Place(_sheet, sheetArea, _selection, !_slotChoice.IsOpen, _comparison.Text, ChangesCaptionId, _notice), opacity);
        if (_slotChoice.Choice is not { } choice)
        {
            _panel.DrawRightEdge(batch, textures.Pixel, MenuLayout.FullHeight(sheetArea), opacity);
            return;
        }

        var choiceArea = new Rectangle(sheetArea.Right, area.Y, EquipmentChoiceView.PanelWidth, area.Height);
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(choiceArea), opacity);
        _choiceView.Draw(batch, textures, choice, choiceArea);
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
        ShowWornComparison();
    }

    private void MoveOrChoose(InputFrame input)
    {
        var rowBefore = _rowCursor.Selected;
        if (input.IsPressed(GameButton.Up))
        {
            _rowCursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _rowCursor.MoveDown();
        }

        _selection.Follow(_rowCursor);
        if (_rowCursor.Selected != rowBefore)
        {
            ShowWornComparison();
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

        var slot = EquipmentSheet.Slots[_rowCursor.Selected];
        var choice = _slotChoice.Open(Shown, slot);
        _choiceView.Show(choice, _messages.Text(EquipmentSheet.LabelIdOf(slot)), _inventory);
    }

    // Once the choice closes, the sheet shows what changed and Changes goes back to the worn item.
    private void UpdateChoice(InputFrame input)
    {
        if (!_slotChoice.Update(input))
        {
            return;
        }

        _notice = _slotChoice.Refusal;
        _sheet = EquipmentSheet.Of(Shown);
        ShowWornComparison();
    }

    // With no choice open, Changes shows what the highlighted slot's item gives; "Remove all" shows nothing.
    private void ShowWornComparison() =>
        _comparison.ShowWorn(Shown, _rowCursor.Selected < EquipmentSheet.Slots.Length ? EquipmentSheet.Slots[_rowCursor.Selected] : null);
}
