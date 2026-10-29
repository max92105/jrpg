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
/// quoted by Core; Salvage asks first (decision 354). Cancel wins when nested: the prompt, then the actions, then the
/// Forge. It fades in when it opens and out when it closes (decision 370): Cancel starts the fade out, and the close
/// request comes once it has run, once, since a second would make the flow pop what lies under it.
/// </summary>
internal sealed class ForgeScene : IScene
{
    private readonly GameSession _session;
    private readonly UiMessages _messages;
    private readonly ForgeList _list;
    private readonly MaterialStock _materials;
    private readonly ForgeActionFormat _actionFormat;
    private readonly ForgeView _view;
    private readonly ForgeMotion _motion = new();
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

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
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

    // Cancel closes the actions before the Forge; Confirm on an available Salvage asks first, on a greyed action or an
    // action not carried out yet (the plan's F2c) it does nothing.
    private void UpdateActions(ForgeActionChoice actions, InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            Actions = null;
            return;
        }

        if (input.IsPressed(GameButton.Confirm) && actions.Selected is { Action: ForgeAction.Salvage, IsAvailable: true } && _list.SelectedRow is { } row)
        {
            Prompt = new ConfirmPrompt(_messages.Format(MessageIds.ForgeSalvageConfirm, new MessageArgument("name", NameOf(row))));
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            actions.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            actions.MoveDown();
        }
    }

    private void UpdateList(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            _motion.StartClosing();
            return;
        }

        if (input.IsPressed(GameButton.Confirm) && _list.SelectedPiece is { } piece)
        {
            Actions = new ForgeActionChoice(piece, _session.Forge, _actionFormat);
            Notice = null;
            return;
        }

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
    }

    // A rolled piece is named with its rarity first, so the question tells it from a plain one of the same base.
    private string NameOf(ForgeRow row) =>
        row.Piece.Rarity == Rarity.Common && row.Piece.Affixes.Count == 0
            ? row.Name.Text
            : _messages.Format(MessageIds.ForgeRolledPieceName, new MessageArgument("rarity", row.RarityName), new MessageArgument("name", row.Name.Text));

    // While closing nothing reads input; the flow hears ForgeClosed once, on the update the fade ends.
    private ForgeClosed? RequestClosedOnceFaded()
    {
        if (!_motion.IsClosingDone || _hasRequestedClose)
        {
            return null;
        }

        _hasRequestedClose = true;
        return new ForgeClosed();
    }
}
