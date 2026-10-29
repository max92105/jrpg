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
/// list, the highlighted piece's affixes and the materials owned beside it. It fades in when it opens and out when it
/// closes (decision 370): Cancel starts the fade out, and the close request comes once it has run, once, since a second
/// would make the flow pop what lies under it.
/// </summary>
internal sealed class ForgeScene : IScene
{
    private readonly ForgeList _list;
    private readonly MaterialStock _materials;
    private readonly ForgeView _view;
    private readonly ForgeMotion _motion = new();
    private bool _hasRequestedClose;

    /// <param name="text">The title, the lines and the rows' fonts.</param>
    /// <param name="ui">The shared textures: icons, the selected row.</param>
    /// <param name="session">Whose owned pieces and materials are listed.</param>
    /// <param name="items">The catalog's items, for the materials among them.</param>
    public ForgeScene(ScreenText text, UiTextures ui, GameSession session, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        var look = new SheetRowLook(text);
        _list = new ForgeList(session.Inventory, text.Messages, text.Fonts.FontAt(ForgeView.BodyPixels));
        _list.Show(0);
        _materials = new MaterialStock(session.Inventory, items, look);
        _view = new ForgeView(text, ui, look);
    }

    /// <summary>The highlighted piece; null when none is owned.</summary>
    public GearPiece? SelectedPiece => _list.SelectedPiece;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _list.Advance(delta);
        _motion.Advance(delta);
        if (_motion.IsClosing)
        {
            return RequestClosedOnceFaded();
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            _motion.StartClosing();
            return null;
        }

        if (input.IsPressed(GameButton.Up))
        {
            _list.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _list.MoveDown();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _motion.Opacity;
        _motion.MarkDrawn();
        _view.Draw(batch, textures, _list, _materials, opacity);
    }

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
