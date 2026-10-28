using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Equipment page's open choice for one slot, from opening to the applied pick: the items the slot can take, the
/// comparison and the footer's take hint following the highlight, and the change asked of Core. Closed until
/// <see cref="Open"/>; one instance serves every slot and member.
/// </summary>
internal sealed class SlotChoice
{
    private readonly GameSession _session;
    private readonly EquipmentComparison _comparison;
    private readonly UiMessages _messages;
    private OpenSlot? _open;
    // The choice row the comparison and the hint were built for; -1 when none was.
    private int _previewedRow = -1;

    /// <param name="session">The party, its equipment and the inventory.</param>
    /// <param name="comparison">The page's Changes band, which follows the highlight while the choice is open.</param>
    /// <param name="messages">The take hint.</param>
    public SlotChoice(GameSession session, EquipmentComparison comparison, UiMessages messages)
    {
        _session = session;
        _comparison = comparison;
        _messages = messages;
    }

    /// <summary>The open choice; null while closed.</summary>
    public EquipmentChoice? Choice => _open?.Choice;

    public bool IsOpen => _open is not null;

    /// <summary>The footer's line while a worn item is highlighted, naming who would lose it; null otherwise.</summary>
    public string? TakeHint { get; private set; }

    /// <summary>
    /// Opens the choice of what <paramref name="member"/> wears in <paramref name="slot"/>, and returns it.
    /// </summary>
    public EquipmentChoice Open(PartyMember member, EquipmentSlot slot)
    {
        var choice = new EquipmentChoice(_session.Equipment.OwnedFor(slot), PartyEquipment.TakeableFor(member, slot, _session.Party));
        _open = new OpenSlot(member, slot, choice);
        ShowPreview(_open);
        return choice;
    }

    /// <summary>
    /// Moves through the choice, or applies the pick; true once the choice closed (picked or cancelled); does nothing
    /// while closed.
    /// </summary>
    public bool Update(InputFrame input)
    {
        if (_open is not { } open)
        {
            return false;
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            Close();
            return true;
        }

        if (open.Choice.Update(input) is { } pick)
        {
            Apply(open, pick);
            Close();
            return true;
        }

        ShowPreview(open);
        return false;
    }

    // Rebuilt when the highlighted row changes, so drawing formats nothing; on "Remove" it compares the empty slot.
    private void ShowPreview(OpenSlot open)
    {
        if (open.Choice.SelectedRow == _previewedRow)
        {
            return;
        }

        _previewedRow = open.Choice.SelectedRow;
        _comparison.ShowChoice(open.Member, open.Slot, open.Choice);
        TakeHint = open.Choice.HighlightedWearer is { } wearer
            ? _messages.Format(MessageIds.EquipmentTakeHint, new MessageArgument("member", wearer.Definition.Name))
            : null;
    }

    private void Close()
    {
        _open = null;
        _previewedRow = -1;
        TakeHint = null;
    }

    // A worn piece is taken from its wearer (decision 137). Core refuses nothing the player needs telling about (gear
    // has no cap, decision 345): Remove on an empty slot, or a piece equal to the worn one, just changes nothing.
    private void Apply(OpenSlot open, EquipmentPick pick)
    {
        var equipment = _session.Equipment;
        var (member, slot, _) = open;
        _ = pick switch
        {
            { Giver: { } giver } => equipment.TryTakeFrom(giver, member, slot),
            { Piece: { } piece } => equipment.TryEquip(member, piece, slot),
            _ => equipment.TryUnequip(member, slot),
        };
    }

    private sealed record OpenSlot(PartyMember Member, EquipmentSlot Slot, EquipmentChoice Choice);
}
