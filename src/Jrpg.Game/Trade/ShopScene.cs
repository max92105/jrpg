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
/// header; the Buy and Sell tabs, which the cursor starts on (Left and Right choose, Confirm enters the shown list,
/// Cancel leaves); in a list Up and Down move the shared selection and Cancel returns to the tabs. It fades in when it
/// opens and out when it closes (decision 370): the close request comes once the fade has run, once, since a second
/// would make the flow pop what lies under it.
/// </summary>
internal sealed class ShopScene : IScene
{
    private readonly GameSession _session;
    private readonly StockList _stock;
    private readonly SaleList _sales;
    private readonly ShopView _view;
    private readonly OverlayFade _fade = new();
    private readonly SelectionMotion _tabPointer = new();
    private bool _hasRequestedClose;

    /// <param name="text">The header's and the footer's fonts and lines.</param>
    /// <param name="ui">The shared textures: the coin, the columns.</param>
    /// <param name="session">Whose gold the header shows, whose bag the Sell list lists.</param>
    /// <param name="shop">The shop opened, from <c>GameSession.ShopFor</c>.</param>
    public ShopScene(ScreenText text, UiTextures ui, GameSession session, Shop shop)
    {
        _session = session;
        var font = text.Fonts.FontAt(ShopView.BodyPixels);
        _stock = new StockList(shop, session.Inventory, font);
        _sales = new SaleList(session.Sales, session.Inventory, session.Catalog.Items, font);
        _view = new ShopView(text, ui, shop.Name);
    }

    /// <summary>The tab shown, whose list is under the tabs.</summary>
    public ShopTab Tab { get; private set; }

    /// <summary>Whether the cursor is on the tabs rather than in the shown list; the Shop opens on them.</summary>
    public bool IsOnTabs { get; private set; } = true;

    /// <summary>The highlighted row of the shown list; null when it has none.</summary>
    public TradeRow? SelectedRow => Tab == ShopTab.Buy ? _stock.SelectedRow : _sales.SelectedRow;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _fade.Advance(delta);
        _tabPointer.Advance(delta);
        ShownSelection.Advance(delta);
        _view.ShowGold(_session.Party.Purse.Gold);
        if (_fade.IsClosing)
        {
            return RequestClosedOnceFaded();
        }

        if (IsOnTabs)
        {
            UpdateTabs(input);
        }
        else
        {
            UpdateList(input);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _fade.Opacity;
        _fade.MarkDrawn();
        var rows = Tab == ShopTab.Buy ? _stock.Rows : _sales.Rows;
        _view.Draw(batch, textures, new ShopListView.Place(Tab, IsOnTabs, _tabPointer, rows, ShownSelection), opacity);
    }

    private RowSelection ShownSelection => Tab == ShopTab.Buy ? _stock.Selection : _sales.Selection;

    // Left and Right choose without wrapping; Confirm enters the shown list; Cancel leaves the Shop.
    private void UpdateTabs(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            _fade.StartClosing();
            return;
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            IsOnTabs = false;
            return;
        }

        if (input.IsPressed(GameButton.Right))
        {
            Tab = ShopTab.Sell;
        }

        if (input.IsPressed(GameButton.Left))
        {
            Tab = ShopTab.Buy;
        }
    }

    // Cancel wins when nested: in a list it only returns to the tabs.
    private void UpdateList(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            IsOnTabs = true;
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            ShownSelection.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            ShownSelection.MoveDown();
        }
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
