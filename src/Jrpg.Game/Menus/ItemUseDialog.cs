using System.Globalization;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Using a consumable from the inventory: pick who it's for among the members Core allows, have Core use it, and say what
/// happened.
/// </summary>
internal sealed class ItemUseDialog
{
    private readonly PartyState _party;
    private readonly FieldItemUse _fieldItems;
    private readonly UiMessages _messages;
    private readonly ItemTargetPickerView _view;
    private OpenUse? _open;

    /// <param name="session">Whose party and inventory the item is used with.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    /// <param name="catalog">Class names, for the member cards.</param>
    public ItemUseDialog(GameSession session, ScreenText text, UiTextures ui, ContentCatalog catalog)
    {
        _party = session.Party;
        _fieldItems = session.FieldItems;
        _messages = text.Messages;
        _view = new ItemTargetPickerView(text, ui, catalog);
    }

    public bool IsOpen => _open is not null;

    /// <exception cref="InvalidOperationException">The item can't be used now (<see cref="FieldItemUse.CanUse"/>).</exception>
    public void Open(ItemDefinition item)
    {
        if (!_fieldItems.CanUse(item))
        {
            throw new InvalidOperationException($"{item.Name} can't be used now.");
        }

        var picker = new ItemTargetPicker(_party.Members, _fieldItems.TargetChoiceForAnyUser(item));
        _open = new OpenUse(item, picker);
        _view.Show(picker, item, _party.Leader);
    }

    /// <summary>Cancel closes the picker; Confirm uses the item.</summary>
    /// <returns>What happened, on the update the item is used; null otherwise.</returns>
    public string? Update(InputFrame input)
    {
        if (_open is null)
        {
            return null;
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            _open = null;
            return null;
        }

        return _open.Picker.Update(input) is { } targets ? Use(_open.Item, targets) : null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, float opacity)
    {
        if (_open is not null)
        {
            _view.Draw(batch, textures, _open.Picker, opacity);
        }
    }

    private string Use(ItemDefinition item, IReadOnlyList<PartyMember> targets)
    {
        var amounts = _fieldItems.UseOn(item, targets);
        _open = null;
        return targets.Count == 1 ? ResultFor(item, targets[0], amounts[0]) : _messages.Format(MessageIds.ItemUsed, new MessageArgument("item", item.Name));
    }

    private string ResultFor(ItemDefinition item, PartyMember target, int amount)
    {
        var messageId = item.Effect?.Kind switch
        {
            EffectKind.RestoreHp => MessageIds.ItemUsedHp,
            EffectKind.RestoreMp => MessageIds.ItemUsedMp,
            EffectKind.Revive => MessageIds.ItemUsedRevive,
            _ => null,
        };
        return messageId is null
            ? _messages.Format(MessageIds.ItemUsed, new MessageArgument("item", item.Name))
            : _messages.Format(
                messageId,
                new MessageArgument("member", target.Definition.Name),
                new MessageArgument("amount", amount.ToString(CultureInfo.InvariantCulture)));
    }

    // The item being used and the picker choosing who for; set and cleared together.
    private sealed record OpenUse(ItemDefinition Item, ItemTargetPicker Picker);
}
