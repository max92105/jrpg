using System.Collections.Immutable;
using FontStashSharp;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The game menu over the map (decision 81): tabs down the left, Confirm enters the selected tab's page and Cancel
/// comes back. Esc is Cancel and Menu at once, so inside a page it only goes back to the tabs; on the tabs either
/// closes the menu.
/// </summary>
internal sealed class GameMenuScene : IScene
{
    // Placeholder layout (decision 82): the header, the body 40 px below it, the tabs (MenuTabsView), the page 24 px
    // right of them, 40 px from the right edge, and 32 px above the 72 px footer.
    private const int FooterHeight = 72;
    private const int BodyTop = MenuHeader.Height + 40;
    private const int PageLeft = NavTabShape.Width + 24;
    private static readonly Rectangle _pageArea = new(PageLeft, BodyTop, VirtualScreen.Width - 40 - PageLeft, VirtualScreen.Height - FooterHeight - 32 - BodyTop);
    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);

    // The footer's hint starts 40 px in.
    private const int FooterTextInset = 40;

    // The party sidebar (question 145 placeholder, the demo's layout): a 520 px column at the page area's right, 24 px
    // from the page.
    private const int SidebarWidth = 520;
    private const int SidebarGap = 24;
    private static readonly Rectangle _pageAreaBesideSidebar = _pageArea with { Width = _pageArea.Width - SidebarWidth - SidebarGap };
    private static readonly Rectangle _sidebarColumn = _pageArea with { X = _pageArea.Right - SidebarWidth, Width = SidebarWidth };

    private readonly UiTextures _ui;
    private readonly MenuHeader _header;
    private readonly PartySidebar _sidebar;
    private readonly PartySidebarView _sidebarView;
    private readonly string _menuTitle;
    private readonly ImmutableArray<IMenuPage> _pages;
    private readonly ListCursor _tabs;
    private readonly SpriteFontBase _font;
    private readonly string[] _tabLabels;
    private readonly MenuTabsView _tabsView;
    private readonly string _mainHint;
    private readonly PageTransition _pageTransition = new();
    private readonly MemberColumn _memberColumn = new();

    // Decision 262: the menu slides in, and fades out before it closes.
    private readonly MenuMotion _motion = new();

    // The selected tab's glow glides between tabs (decision 262).
    private readonly SelectionGlide _tabGlide = new(0);
    private bool _hasRequestedClose;
    private bool _isInPage;

    /// <param name="text">Messages and fonts for the tab labels.</param>
    /// <param name="ui">The shared tab and gradient textures.</param>
    /// <param name="frame">The header bar and the party sidebar with its view.</param>
    /// <param name="pages">One per tab, top to bottom.</param>
    public GameMenuScene(ScreenText text, UiTextures ui, MenuFrame frame, ImmutableArray<IMenuPage> pages)
    {
        _ui = ui;
        _header = frame.Header;
        _sidebar = frame.Sidebar;
        _sidebarView = frame.SidebarView;
        _menuTitle = text.Messages.Text(MessageIds.MenuTitle);
        _pages = pages;
        _tabs = new ListCursor(pages.Length, _ => true);
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _tabLabels = [.. pages.Select(page => text.Messages.Text(page.TitleMessageId))];
        _tabsView = new MenuTabsView(ui, _font, _tabLabels);
        _mainHint = text.Messages.Text(MessageIds.MenuHintMain);
    }

    /// <summary>The footer's line: the open page's hint, or the tabs' own.</summary>
    internal string FooterHint => _isInPage ? _pages[_tabs.Selected].Hint : _mainHint;

    /// <summary>Whether the party sidebar shows: only beside an open page that wants it.</summary>
    internal bool ShowsSidebar => _isInPage && _pages[_tabs.Selected].ShowsPartySidebar;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _motion.Advance(delta);
        _tabGlide.Advance(delta);
        if (_motion.IsClosing)
        {
            return RequestClosedOnceFaded();
        }

        _pageTransition.Update(delta);
        _header.Update();
        SceneRequest? request = null;
        if (_isInPage)
        {
            _pages[_tabs.Selected].Advance(delta);
            request = UpdatePage(input);
        }
        else
        {
            UpdateTabs(input);
        }

        if (_isInPage)
        {
            RefreshMemberViews(_pages[_tabs.Selected] as IMemberPage);
        }

        return request;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _motion.FrameOpacity;
        _motion.MarkDrawn();
        DrawFrame(batch, textures, opacity);
        _header.Draw(batch, _isInPage ? _tabLabels[_tabs.Selected] : _menuTitle, opacity);
        _tabsView.Draw(batch, textures, new MenuTabsView.Place(BodyTop, _tabs.Selected, _tabGlide, _motion));

        if (_isInPage)
        {
            var pageArea = ShowsSidebar ? _pageAreaBesideSidebar : _pageArea;
            var area = pageArea with { X = pageArea.X + _pageTransition.Offset };
            _pages[_tabs.Selected].Draw(batch, textures, area, opacity * _pageTransition.Opacity);
        }

        if (ShowsSidebar)
        {
            var pointsAtShown = _pages[_tabs.Selected] is not IMemberPage { HasOwnPointer: true };
            _sidebarView.Draw(batch, textures, new PartySidebarView.Place(_sidebarColumn, pointsAtShown), opacity * _pageTransition.Opacity);
        }

        DrawFooterHint(batch, opacity);
    }

    private void DrawFooterHint(SpriteBatch batch, float opacity)
    {
        var footer = FooterBar.Area(FooterHeight);
        var top = footer.Y + ((footer.Height - _font.LineHeight) / 2);
        batch.DrawString(_font, FooterHint, new Vector2(FooterTextInset, top), UiTheme.TextSecondary * opacity);
    }

    // The picture with its edges darkened, between the header and footer bars.
    private void DrawFrame(SpriteBatch batch, TextureLibrary textures, float opacity)
    {
        var tint = Color.White * opacity;
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Menu), tint);
        batch.Draw(_ui.MenuScrim(textures), _screen, tint);

        HeaderBar.Draw(batch, textures.Pixel, MenuHeader.Height, opacity);
        FooterBar.Draw(batch, textures.Pixel, FooterHeight, opacity);
    }

    // Input works during the slide-in, so a quick player isn't slowed; Cancel or Menu starts the closing fade.
    private void UpdateTabs(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel) || input.IsPressed(GameButton.Menu))
        {
            _motion.StartClosing();
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            _tabs.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _tabs.MoveDown();
        }

        _tabGlide.Follow(_tabs.Selected, _tabLabels.Length);

        if (input.IsPressed(GameButton.Confirm))
        {
            _pages[_tabs.Selected].Open();
            _pageTransition.Start();
            _isInPage = true;
        }
    }

    // The sidebar marks the member a page shows; on a member page, the member column follows the page too.
    private void RefreshMemberViews(IMemberPage? memberPage)
    {
        _sidebar.Refresh(memberPage?.Shown);
        if (memberPage is not null)
        {
            _memberColumn.Refresh(memberPage.Members, memberPage.Shown);
        }
    }

    // While closing nothing reads input; the flow hears MenuClosed once, on the update the fade ends.
    private MenuClosed? RequestClosedOnceFaded()
    {
        if (!_motion.IsClosingDone || _hasRequestedClose)
        {
            return null;
        }

        _hasRequestedClose = true;
        return new MenuClosed();
    }

    private SceneRequest? UpdatePage(InputFrame input)
    {
        var page = _pages[_tabs.Selected];
        if (input.IsPressed(GameButton.Cancel) && page.LeavesOnCancel)
        {
            _isInPage = false;
            return null;
        }

        return page.Update(input);
    }
}
