using System.Diagnostics.CodeAnalysis;
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
/// The menu's Equipment page: one member at a time, what they wear in each slot, a "Remove all" action, and their
/// parameters. Changing what's worn comes in P3b.
/// </summary>
internal sealed class EquipmentPage : IMemberPage
{
    // The slots, then "Remove all".
    private static readonly int _rowCount = EquipmentSheet.Slots.Length + 1;

    private readonly PartyState _party;
    private readonly PartyEquipment _equipment;
    private readonly Inventory _inventory;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private readonly UiMessages _messages;
    private readonly string _pageHint;
    private readonly EquipmentSheetView _sheetView;
    private readonly EquipmentChoiceView _choiceView;
    private readonly ListCursor _memberCursor;
    private readonly ListCursor _rowCursor = new(_rowCount, _ => true);

    // The sheet's selected row glides between rows (decision 262); the choice has its own.
    private readonly SelectionGlide _rowGlide = new(0);
    private EquipmentSheet _sheet;
    private EquipmentChoice? _choice;
    private StatPreviewText? _preview;
    // The choice row the preview was built for; -1 when none was.
    private int _previewedRow = -1;
    private string? _notice;
    // The footer's line while a worn item is highlighted, naming who would lose it; null otherwise.
    private string? _takeHint;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: section lines, icons, the selected row.</param>
    /// <param name="session">Whose party and inventory.</param>
    /// <param name="catalog">The items, to list what each slot can take.</param>
    public EquipmentPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        _pageHint = text.Messages.Text(MessageIds.MenuHintEquipment);
        _party = session.Party;
        _equipment = session.Equipment;
        _inventory = session.Inventory;
        _items = catalog.Items;
        _messages = text.Messages;
        _sheetView = new EquipmentSheetView(text, ui);
        _choiceView = new EquipmentChoiceView(text, ui);
        _memberCursor = new ListCursor(_party.Members.Count, _ => true);
        Open();
    }

    /// <summary>Everyone recruited: the active party in its order, then the reserve.</summary>
    public IReadOnlyList<PartyMember> Members { get; private set; }

    public PartyMember Shown => Members[_memberCursor.Selected];

    public string TitleMessageId => MessageIds.MenuEquipment;

    public string Hint => _takeHint ?? _pageHint;

    // Recruits don't change while the menu is open, so the member cursor's count still holds.
    [MemberNotNull(nameof(Members), nameof(_sheet))]
    public void Open()
    {
        Members = MemberOrder.ActiveThenReserve(_party);
        _memberCursor.Select(0);
        _rowCursor.Select(0);
        _rowGlide.JumpTo(0);
        _sheet = EquipmentSheet.Of(Shown);
        _notice = null;
    }

    /// <summary>False while choosing what goes in a slot: Cancel closes that instead.</summary>
    public bool LeavesOnCancel => _choice is null;

    /// <summary>Whether the player is choosing what goes in a slot.</summary>
    public bool IsChoosing => _choice is not null;

    /// <summary>The open choice, so tests can pick a row by what it holds; null while no slot is being chosen for.</summary>
    internal EquipmentChoice? Choice => _choice;

    /// <summary>Why the last change didn't happen, or null.</summary>
    public string? Notice => _notice;

    /// <summary>Where the sheet's selected row band is, in rows, as it glides.</summary>
    internal float SelectionPosition => _rowGlide.Position;

    public void Advance(TimeSpan delta)
    {
        _rowGlide.Advance(delta);
        _choice?.Selection.Advance(delta);
    }

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

    // The choice opens as a panel at the right, and the sheet narrows to make room for it.
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var sheetWidth = _choice is null ? area.Width : area.Width - EquipmentChoiceView.PanelWidth;
        var sheetArea = area with { Width = sheetWidth };
        _sheetView.Draw(batch, textures, new EquipmentSheetView.Place(_sheet, sheetArea, _rowGlide, _choice is null, _preview, _notice), opacity);
        if (_choice is not null)
        {
            _choiceView.Draw(batch, textures, _choice, new Rectangle(sheetArea.Right, area.Y, EquipmentChoiceView.PanelWidth, area.Height));
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

        _rowGlide.Follow(_rowCursor.Selected, _rowCount);

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
        _choice = new EquipmentChoice(_equipment.OwnedFor(slot, _items), PartyEquipment.TakeableFor(Shown, slot, _party));
        _choiceView.Show(_choice, _messages.Text(EquipmentSheet.LabelIdOf(slot)), _inventory);
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
        _takeHint = choice.HighlightedWearer is { } wearer
            ? _messages.Format(MessageIds.EquipmentTakeHint, new MessageArgument("member", wearer.Definition.Name))
            : null;
    }

    private void CloseChoice()
    {
        _choice = null;
        _preview = null;
        _previewedRow = -1;
        _takeHint = null;
    }

    // Core refuses a change when the item it would take off has no room left in the inventory; the player is told why.
    // A worn item is taken from its wearer (decision 137).
    private void Apply(EquipmentPick pick, EquipmentSlot slot)
    {
        var changed = pick switch
        {
            { Giver: { } giver } => _equipment.TryTakeFrom(giver, Shown, slot),
            { Item: { } item } => _equipment.TryEquip(Shown, item),
            _ => _equipment.TryUnequip(Shown, slot),
        };
        if (!changed && Shown.EquippedIn(slot) is { } worn && !_inventory.HasRoomFor(worn))
        {
            _notice = _messages.Format(MessageIds.EquipmentNoRoom, new MessageArgument("item", worn.Name));
        }

        _sheet = EquipmentSheet.Of(Shown);
    }
}
