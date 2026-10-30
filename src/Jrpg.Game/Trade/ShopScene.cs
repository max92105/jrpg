using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Core.Trade;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Trade;

/// <summary>
/// The Shop screen a shopkeeper's event opens over the map (decision 353): the shop's name and the party's gold in the
/// header; the Buy and Sell tabs, which the cursor starts on (Left and Right choose, Confirm enters the shown list,
/// Cancel leaves); in a list Up and Down move the shared selection, the tab buttons change its category (decision 384,
/// each tab keeping its own), Left and Right switch the member whose changes a gear row previews (decision 383) and
/// Cancel returns to the tabs. It fades in when it opens and out when it closes (decision 370): the close request comes
/// once the fade has run, once, since a second would make the flow pop what lies under it.
/// </summary>
internal sealed class ShopScene : IScene
{
    private readonly GameSession _session;
    private readonly StockList _stock;
    private readonly SaleList _sales;
    private readonly CategoryRail _buyRail = new();
    private readonly CategoryRail _sellRail = new();
    private readonly EquipmentComparison _comparison;
    private readonly ShopView _view;
    private readonly OverlayFade _fade = new();
    private readonly SelectionMotion _tabPointer = new();
    private int _memberIndex;
    private TradeRow? _previewedRow;
    private PartyMember? _previewedMember;
    private bool _hasRequestedClose;

    /// <param name="text">The header's and the footer's fonts and lines.</param>
    /// <param name="ui">The shared textures: the coin, the columns.</param>
    /// <param name="session">
    /// Whose gold the header shows, whose bag the Sell list lists, whose party is previewed.
    /// </param>
    /// <param name="shop">The shop opened, from <c>GameSession.ShopFor</c>.</param>
    public ShopScene(ScreenText text, UiTextures ui, GameSession session, Shop shop)
    {
        _session = session;
        _stock = new StockList(shop, session.Inventory, text);
        _sales = new SaleList(session.Sales, session.Inventory, session.Catalog.Items, text);
        _comparison = new EquipmentComparison(text.Messages);
        _view = new ShopView(text, ui, shop.Name);
    }

    /// <summary>The tab shown, whose list is under the tabs.</summary>
    public ShopTab Tab { get; private set; }

    /// <summary>Whether the cursor is on the tabs rather than in the shown list; the Shop opens on them.</summary>
    public bool IsOnTabs { get; private set; } = true;

    /// <summary>The shown tab's category.</summary>
    public InventoryCategory Category => ShownRail.Category;

    /// <summary>The highlighted row of the shown list; null when it has none.</summary>
    public TradeRow? SelectedRow => Tab == ShopTab.Buy ? _stock.SelectedRow : _sales.SelectedRow;

    /// <summary>The active member whose changes a gear row previews; the leader first (decision 383).</summary>
    public PartyMember PreviewedMember => _session.Party.ActiveMembers[_memberIndex];

    /// <summary>
    /// What wearing the highlighted piece would change for <see cref="PreviewedMember"/>; null on other rows.
    /// </summary>
    public StatComparisonText? Preview => _comparison.Text;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _fade.Advance(delta);
        _tabPointer.Advance(delta);
        ShownSelection.Advance(delta);
        ShownRail.Selection.Advance(delta);
        _view.ShowGold(_session.Party.Purse.Gold);
        _view.ShowDevice(input.Device);
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

        RefreshPreview();
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _fade.Opacity;
        _fade.MarkDrawn();
        var rows = Tab == ShopTab.Buy ? _stock.Rows : _sales.Rows;
        var list = new ShopListView.Place(Tab, ShownRail, IsOnTabs, _tabPointer, rows, ShownSelection);
        var detail = new ShopDetailView.Place(SelectedRow, Preview is null ? null : PreviewedMember, Preview, Notice: null);
        _view.Draw(batch, textures, new ShopView.Place(list, detail), opacity);
    }

    private RowSelection ShownSelection => Tab == ShopTab.Buy ? _stock.Selection : _sales.Selection;

    private CategoryRail ShownRail => Tab == ShopTab.Buy ? _buyRail : _sellRail;

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

    // Cancel wins when nested: in a list it only returns to the tabs. The tab buttons change the category, the cursor
    // going to its first row; Left and Right switch the previewed member, wrapping through the active party.
    private void UpdateList(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            IsOnTabs = true;
            return;
        }

        if (TryChangeCategory(input))
        {
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

        var members = _session.Party.ActiveMembers.Count;
        if (input.IsPressed(GameButton.Right))
        {
            _memberIndex = (_memberIndex + 1) % members;
        }

        if (input.IsPressed(GameButton.Left))
        {
            _memberIndex = (_memberIndex + members - 1) % members;
        }
    }

    // The tab buttons move the shown rail (the shoulders, Q and R); any other press isn't a category change.
    private bool TryChangeCategory(InputFrame input)
    {
        if (input.IsPressed(GameButton.PreviousTab))
        {
            ShownRail.MoveUp();
        }
        else if (input.IsPressed(GameButton.NextTab))
        {
            ShownRail.MoveDown();
        }
        else
        {
            return false;
        }

        ShowCategoryFirstRow();
        return true;
    }

    private void ShowCategoryFirstRow()
    {
        if (Tab == ShopTab.Buy)
        {
            _stock.Refresh(ShownRail.Category);
        }
        else
        {
            _sales.Refresh(ShownRail.Category);
        }
    }

    // Rebuilt only when the row or the member changes, never per frame; a stack or a material has no preview.
    private void RefreshPreview()
    {
        var row = SelectedRow;
        var member = PreviewedMember;
        if (ReferenceEquals(row, _previewedRow) && ReferenceEquals(member, _previewedMember))
        {
            return;
        }

        _previewedRow = row;
        _previewedMember = member;
        if (row?.Piece is { } piece)
        {
            _comparison.ShowPiece(member, piece);
        }
        else
        {
            _comparison.Clear();
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
