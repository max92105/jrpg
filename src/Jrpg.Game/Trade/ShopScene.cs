using Jrpg.Core.Session;
using Jrpg.Core.Trade;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Trade;

/// <summary>
/// The Shop screen a shopkeeper's event opens over the map (decision 353): the shop's name and the party's gold in the
/// header. It fades in when it opens and out when it closes (decision 370): Cancel starts the fade out, and the close
/// request comes once it has run, once, since a second would make the flow pop what lies under it.
/// </summary>
internal sealed class ShopScene : IScene
{
    private readonly GameSession _session;
    private readonly ShopView _view;
    private readonly OverlayFade _fade = new();
    private bool _hasRequestedClose;

    /// <param name="text">The header's and the footer's fonts and lines.</param>
    /// <param name="ui">The shared textures: the coin, the columns.</param>
    /// <param name="session">Whose gold the header shows.</param>
    /// <param name="shop">The shop opened, from <c>GameSession.ShopFor</c>.</param>
    public ShopScene(ScreenText text, UiTextures ui, GameSession session, Shop shop)
    {
        _session = session;
        _view = new ShopView(text, ui, shop.Name);
    }

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _fade.Advance(delta);
        _view.ShowGold(_session.Party.Purse.Gold);
        if (_fade.IsClosing)
        {
            return RequestClosedOnceFaded();
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            _fade.StartClosing();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _fade.Opacity;
        _fade.MarkDrawn();
        _view.Draw(batch, textures, opacity);
    }

    // While closing nothing reads input; the flow hears the close once, on the update the fade ends.
    private EventScreenClosed? RequestClosedOnceFaded()
    {
        if (!_fade.IsClosingDone || _hasRequestedClose)
        {
            return null;
        }

        _hasRequestedClose = true;
        return new EventScreenClosed();
    }
}
