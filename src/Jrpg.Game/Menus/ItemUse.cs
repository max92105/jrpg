using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// Using a consumable from the inventory: pick who it's for among the members Core allows, on the party sidebar's cards
/// (decision 267), have Core use it, and say what happened. Aiming goes on after a use while copies are left and
/// someone can take the item (decision 288). The page draws the aiming; this holds it.
/// </summary>
internal sealed class ItemUse
{
    private readonly PartyState _party;
    private readonly FieldItemUse _fieldItems;
    private readonly UiMessages _messages;
    private Aim? _aim;

    /// <param name="session">Whose party and inventory the item is used with.</param>
    /// <param name="messages">The result lines and the target hint.</param>
    public ItemUse(GameSession session, UiMessages messages)
    {
        _party = session.Party;
        _fieldItems = session.FieldItems;
        _messages = messages;
    }

    public bool IsOpen => _aim is not null;

    /// <summary>Who the item is being aimed at, in the sidebar's order; null while not aiming.</summary>
    public ItemTargetPicker? Picker => _aim?.Picker;

    /// <summary>The footer's line while aiming, naming the item; formatted once when aiming starts.</summary>
    public string? Hint => _aim?.Hint;

    /// <summary>The HP the aimed item would leave <paramref name="member"/> with, as Core works it out.</summary>
    /// <param name="member">One of <see cref="Picker"/>'s candidates.</param>
    /// <exception cref="InvalidOperationException">No item is aimed.</exception>
    /// <exception cref="KeyNotFoundException">The member can't take the item.</exception>
    public int HpAfter(PartyMember member) =>
        (_aim ?? throw new InvalidOperationException("No item is aimed, so there is no HP to preview.")).HpAfter[member];

    /// <summary>Starts aiming at the sidebar, or uses a rest item at once: it takes the whole party.</summary>
    /// <returns>What happened when the item was used at once; null when aiming started.</returns>
    /// <exception cref="InvalidOperationException">The item can't be used now (<see cref="FieldItemUse.CanUse"/>).</exception>
    public string? UseOrPickTarget(ItemDefinition item)
    {
        if (!_fieldItems.CanUse(item))
        {
            throw new InvalidOperationException($"{item.Name} can't be used now.");
        }

        if (item.Effect?.Kind == EffectKind.Rest)
        {
            _fieldItems.RestParty(item);
            return _messages.Text(MessageIds.PartyRests);
        }

        var hint = _messages.Format(MessageIds.MenuHintInventoryTarget, new MessageArgument("item", item.Name));
        _aim = AimFor(item, hint);
        return null;
    }

    /// <summary>Cancel stops aiming; Confirm uses the item.</summary>
    /// <returns>What happened, on the update the item is used; null otherwise.</returns>
    public string? Update(InputFrame input)
    {
        if (_aim is null)
        {
            return null;
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            _aim = null;
            return null;
        }

        return _aim.Picker.Update(input) is { } targets ? Use(_aim, targets) : null;
    }

    private string Use(Aim aim, IReadOnlyList<PartyMember> targets)
    {
        var item = aim.Item;
        var amounts = _fieldItems.UseOn(item, targets);
        AimAgainOrStop(aim, targets[0]);
        return targets.Count == 1 ? ResultFor(item, targets[0], amounts[0]) : _messages.Format(MessageIds.ItemUsed, new MessageArgument("item", item.Name));
    }

    // Decision 288: while copies are left and someone can take the item, aiming goes on, on the same member when they
    // still can, else on the first who can; otherwise it ends and the cursor is back on the list.
    private void AimAgainOrStop(Aim aim, PartyMember lastTarget)
    {
        if (!_fieldItems.CanUse(aim.Item))
        {
            _aim = null;
            return;
        }

        var aimAgain = AimFor(aim.Item, aim.Hint);
        aimAgain.Picker.PointAt(lastTarget);
        _aim = aimAgain;
    }

    // In the party sidebar's order, so the pointer lands on the card it names; each candidate's HP after the item is
    // Core's, asked once here so the previews (decision 287) follow every use.
    private Aim AimFor(ItemDefinition item, string hint)
    {
        var picker = new ItemTargetPicker(MemberOrder.ActiveThenReserve(_party), _fieldItems.TargetChoiceForAnyUser(item));
        var hpAfter = picker.Members
            .Where(picker.IsCandidate)
            .ToDictionary(member => member, member => FieldItemUse.HpAfterUse(item, member));
        return new Aim(item, picker, hint, hpAfter);
    }

    private string ResultFor(ItemDefinition item, PartyMember target, int amount)
    {
        var messageId = item.Effect?.Kind switch
        {
            EffectKind.RestoreHp => MessageIds.ItemUsedHp,
            EffectKind.Revive => MessageIds.ItemUsedRevive,
            _ => null,
        };
        return messageId is null
            ? _messages.Format(MessageIds.ItemUsed, new MessageArgument("item", item.Name))
            : _messages.Format(
                messageId,
                new MessageArgument("member", target.Definition.Name),
                MessageArgument.Number("amount", amount));
    }

    // The item aimed, who at, the footer's line naming it, each candidate's HP after it; set and cleared together.
    private sealed record Aim(ItemDefinition Item, ItemTargetPicker Picker, string Hint, IReadOnlyDictionary<PartyMember, int> HpAfter);
}
