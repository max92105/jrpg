using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// Using a consumable from the inventory: pick who it's for among the members Core allows, on the party sidebar's cards
/// (decision 267), have Core use it, and say what happened. The page draws the aiming; this holds it.
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

        var picker = new ItemTargetPicker(MemberOrder.ActiveThenReserve(_party), _fieldItems.TargetChoiceForAnyUser(item));
        var hint = _messages.Format(MessageIds.MenuHintInventoryTarget, new MessageArgument("item", item.Name));
        _aim = new Aim(item, picker, hint);
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

        return _aim.Picker.Update(input) is { } targets ? Use(_aim.Item, targets) : null;
    }

    private string Use(ItemDefinition item, IReadOnlyList<PartyMember> targets)
    {
        var amounts = _fieldItems.UseOn(item, targets);
        _aim = null;
        return targets.Count == 1 ? ResultFor(item, targets[0], amounts[0]) : _messages.Format(MessageIds.ItemUsed, new MessageArgument("item", item.Name));
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

    // The item being aimed, who at, and the footer's line naming it; set and cleared together.
    private sealed record Aim(ItemDefinition Item, ItemTargetPicker Picker, string Hint);
}
