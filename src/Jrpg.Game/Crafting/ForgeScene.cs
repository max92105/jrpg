using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// The Forge screen a forge master's event opens over the map (decision 353): the owned pieces, Up and Down moving the
/// list, the highlighted piece's affixes and the materials owned beside it. Confirm on a piece opens its actions,
/// quoted by Core; Salvage asks first (decision 354), an upgrade or a reroll goes at once. Cancel wins when nested: the
/// prompt, then the actions, then the Forge. It fades in when it opens and out when it closes (decision 370): Cancel
/// starts the fade out, and the close request comes once it has run, once, since a second would make the flow pop what
/// lies under it.
/// </summary>
internal sealed class ForgeScene : IScene
{
    private readonly GameSession _session;
    private readonly UiMessages _messages;
    private readonly ForgeList _list;
    private readonly MaterialStock _materials;
    private readonly ForgeActionFormat _actionFormat;
    private readonly ForgeView _view;
    private readonly OverlayFade _motion = new();
    private readonly AskedSounds _sounds = new();
    private bool _hasRequestedClose;

    /// <param name="text">The title, the lines and the rows' fonts.</param>
    /// <param name="ui">The shared textures: icons, the selected row.</param>
    /// <param name="session">Whose owned pieces and materials are listed, and whose Forge quotes the actions.</param>
    /// <param name="items">The catalog's items, for the materials among them and their names.</param>
    public ForgeScene(ScreenText text, UiTextures ui, GameSession session, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        _session = session;
        _messages = text.Messages;
        var look = new SheetRowLook(text);
        var bodyFont = text.Fonts.FontAt(ForgeView.BodyPixels);
        _list = new ForgeList(session.Inventory, text.Messages, bodyFont);
        _list.Show(0);
        _materials = new MaterialStock(session.Inventory, items, look);
        _actionFormat = new ForgeActionFormat(text.Messages, items, bodyFont);
        _view = new ForgeView(text, ui, look);
    }

    /// <summary>The highlighted piece; null when none is owned.</summary>
    public GearPiece? SelectedPiece => _list.SelectedPiece;

    /// <summary>The actions open on the highlighted piece; null while the list has the cursor.</summary>
    public ForgeActionChoice? Actions { get; private set; }

    /// <summary>The question before a salvage; null while nothing is asked.</summary>
    public ConfirmPrompt? Prompt { get; private set; }

    /// <summary>
    /// What the last action gave, shown until the cursor moves or another piece's actions open; null when nothing was
    /// done since.
    /// </summary>
    public string? Notice { get; private set; }

    public IReadOnlyList<string> Sounds => _sounds.Files;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _sounds.Clear();
        _list.Advance(delta);
        Actions?.Advance(delta);
        _motion.Advance(delta);
        if (_motion.IsClosing)
        {
            return RequestClosedOnceFaded();
        }

        if (Prompt is { } prompt)
        {
            Answer(prompt, input);
        }
        else if (Actions is { } actions)
        {
            UpdateActions(actions, input);
        }
        else
        {
            UpdateList(input);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _motion.Opacity;
        _motion.MarkDrawn();
        _view.Draw(batch, textures, new ForgeView.Place(_list, _materials, Actions, Notice, Prompt), opacity);
    }

    // Yes salvages; No, and Cancel, keep the piece and the actions open.
    private void Answer(ConfirmPrompt prompt, InputFrame input)
    {
        var answer = prompt.Update(input);
        if (answer is null)
        {
            return;
        }

        Prompt = null;
        if (answer == ConfirmAnswer.Yes && Actions is { } actions && _list.SelectedRow is { } row)
        {
            Salvage(row, actions.Selected);
        }
    }

    // The piece leaves the list and its yield joins the materials; the cursor stays on the row, or the last one.
    private void Salvage(ForgeRow row, ForgeActionRow salvage)
    {
        if (!_session.Forge.TrySalvage(row.Piece))
        {
            return;
        }

        Actions = null;
        _list.Show(_list.Cursor?.Selected ?? 0);
        _materials.Refresh();
        Notice = _messages.Format(MessageIds.ForgeSalvaged, new MessageArgument("materials", salvage.Materials.Text));
    }

    // Cancel closes the actions before the Forge; Confirm carries out the highlighted action, or nothing on a greyed
    // one.
    private void UpdateActions(ForgeActionChoice actions, InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            _sounds.Ask(UiSoundFiles.Effects.MenuCancel);
            Actions = null;
            return;
        }

        if (input.IsPressed(GameButton.Confirm) && actions.Selected is { IsAvailable: true } action && _list.SelectedRow is { } row)
        {
            CarryOut(action.Action, row);
            return;
        }

        var before = actions.SelectedIndex;
        if (input.IsPressed(GameButton.Up))
        {
            actions.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            actions.MoveDown();
        }

        _sounds.AskMoveIfChanged(before, actions.SelectedIndex);
    }

    // Salvage destroys, so it asks first (decision 354); an upgrade or a reroll only changes the piece, so it goes at
    // once.
    private void CarryOut(ForgeAction action, ForgeRow row)
    {
        switch (action)
        {
            case ForgeAction.Salvage:
                Prompt = new ConfirmPrompt(_messages.Format(MessageIds.ForgeSalvageConfirm, new MessageArgument("name", PieceName.Of(row.Piece, _messages))));
                break;
            case ForgeAction.Upgrade:
                Craft(row, _session.Forge.TryUpgrade, MessageIds.ForgeUpgraded);
                break;
            case ForgeAction.Reroll:
                Craft(row, _session.Forge.TryReroll, MessageIds.ForgeRerolled);
                break;
        }
    }

    // The piece is replaced in its place; with two equal pieces the first one changes (Inventory.TryReplace), so the
    // highlight goes to that row, and the actions open again on the new piece with its new quotes.
    private void Craft(ForgeRow row, Func<GearPiece, bool> tryCraft, string noticeId)
    {
        var gear = _session.Inventory.Gear;
        var index = Enumerable.Range(0, gear.Count).First(candidate => gear[candidate] == row.Piece);
        if (!tryCraft(row.Piece))
        {
            return;
        }

        _sounds.Ask(UiSoundFiles.Effects.MenuConfirm);
        _list.Show(index);
        _materials.Refresh();
        var crafted = _list.SelectedRow ?? throw new InvalidOperationException("The crafted piece left the list.");
        Actions = new ForgeActionChoice(crafted.Piece, _session.Forge, _actionFormat);
        Notice = _messages.Format(noticeId, new MessageArgument("name", row.Name.Text), new MessageArgument("rarity", crafted.RarityName));
    }

    private void UpdateList(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            _sounds.Ask(UiSoundFiles.Effects.MenuCancel);
            _motion.StartClosing();
            return;
        }

        if (input.IsPressed(GameButton.Confirm) && _list.SelectedPiece is { } piece)
        {
            _sounds.Ask(UiSoundFiles.Effects.MenuConfirm);
            Actions = new ForgeActionChoice(piece, _session.Forge, _actionFormat);
            Notice = null;
            return;
        }

        var before = _list.Cursor?.Selected ?? 0;
        if (input.IsPressed(GameButton.Up))
        {
            _list.MoveUp();
            Notice = null;
        }

        if (input.IsPressed(GameButton.Down))
        {
            _list.MoveDown();
            Notice = null;
        }

        _sounds.AskMoveIfChanged(before, _list.Cursor?.Selected ?? 0);
    }

    // While closing nothing reads input; the flow hears EventScreenClosed once, on the update the fade ends.
    private EventScreenClosed? RequestClosedOnceFaded()
    {
        if (!_motion.IsClosingDone || _hasRequestedClose)
        {
            return null;
        }

        _hasRequestedClose = true;
        return new EventScreenClosed();
    }
}
