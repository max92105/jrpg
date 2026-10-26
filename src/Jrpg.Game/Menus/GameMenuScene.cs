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
    // The tabs don't move yet, so the page area beside them is the same every frame.
    private static readonly Rectangle _pageArea = MenuLayout.PageArea(NavTabShape.Width);
    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);

    // The footer's hint starts 40 px in.
    private const int FooterTextInset = 40;

    private readonly UiTextures _ui;
    private readonly MenuTabsView _tabsView;
    private readonly MemberColumnView _memberColumnView;
    private readonly MenuHeader _header;
    private readonly string _menuTitle;
    private readonly ImmutableArray<IMenuPage> _pages;
    private readonly ListCursor _tabs;
    private readonly SpriteFontBase _font;
    private readonly string[] _tabLabels;
    private readonly string _mainHint;
    private readonly PageTransition _pageTransition = new();

    // Decision 262: the menu slides in, and fades out before it closes.
    private readonly MenuMotion _motion = new();

    // The selected tab's glow glides between tabs (decision 262).
    private readonly SelectionGlide _tabGlide = new(0);
    private bool _hasRequestedClose;
    private bool _isInPage;

    /// <param name="text">Messages and fonts for the tab labels.</param>
    /// <param name="ui">The shared tab and gradient textures.</param>
    /// <param name="header">The header bar: the page title, the place and the play time.</param>
    /// <param name="pages">One per tab, top to bottom.</param>
    public GameMenuScene(ScreenText text, UiTextures ui, MenuHeader header, ImmutableArray<IMenuPage> pages)
    {
        _ui = ui;
        _header = header;
        _menuTitle = text.Messages.Text(MessageIds.MenuTitle);
        _pages = pages;
        _tabs = new ListCursor(pages.Length, _ => true);
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _tabLabels = [.. pages.Select(page => text.Messages.Text(page.TitleMessageId))];
        _tabsView = new MenuTabsView(ui, _font, [.. pages.Select((page, index) => new MenuTabsView.Tab(_tabLabels[index], page.Glyph))], MenuLayout.BodyTop);
        _memberColumnView = new MemberColumnView(ui);
        _mainHint = text.Messages.Text(MessageIds.MenuHintMain);
    }

    /// <summary>The footer's line: the open page's hint, or the tabs' own.</summary>
    internal string FooterHint => _isInPage ? _pages[_tabs.Selected].Hint : _mainHint;

    /// <summary>The members a member page goes through and the one it shows, kept current while it is open.</summary>
    internal MemberColumn MemberColumn { get; } = new();

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
            RefreshMemberColumn();
        }

        return request;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _motion.FrameOpacity;
        _motion.MarkDrawn();
        DrawFrame(batch, textures, opacity);
        _header.Draw(batch, _isInPage ? _tabLabels[_tabs.Selected] : _menuTitle, opacity);
        _tabsView.Draw(batch, textures, new MenuTabsView.Place(_motion, _tabs.Selected, _tabGlide.Position));
        if (_isInPage)
        {
            DrawPage(batch, textures, opacity * _pageTransition.Opacity);
        }

        DrawFooterHint(batch, opacity);
    }

    private void RefreshMemberColumn()
    {
        if (_pages[_tabs.Selected] is IMemberPage memberPage)
        {
            MemberColumn.Refresh(memberPage.Members, memberPage.Shown);
        }
    }

    private void DrawFooterHint(SpriteBatch batch, float opacity)
    {
        var top = FooterBar.TextTop(MenuLayout.FooterHeight, _font.LineHeight);
        batch.DrawString(_font, FooterHint, new Vector2(FooterTextInset, top), UiTheme.TextSecondary * opacity);
    }

    // The picture with its edges darkened, between the header and footer bars.
    private void DrawFrame(SpriteBatch batch, TextureLibrary textures, float opacity)
    {
        var tint = Color.White * opacity;
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Menu), tint);
        batch.Draw(_ui.MenuScrim(textures), _screen, tint);

        HeaderBar.Draw(batch, textures.Pixel, MenuHeader.Height, opacity);
        FooterBar.Draw(batch, textures.Pixel, MenuLayout.FooterHeight, opacity);
    }

    // A member page gets the member column at its left and narrows by it; both slide in with the page transition.
    private void DrawPage(SpriteBatch batch, TextureLibrary textures, float opacity)
    {
        var area = _pageArea with { X = _pageArea.X + _pageTransition.Offset };
        var page = _pages[_tabs.Selected];
        if (page is IMemberPage memberPage)
        {
            var column = area with { Width = MemberColumnView.Width };
            _memberColumnView.Draw(batch, textures, new MemberColumnView.Place(MemberColumn, column, !memberPage.HasOwnPointer), opacity);
            area = area with { X = column.Right, Width = area.Width - MemberColumnView.Width };
        }

        page.Draw(batch, textures, area, opacity);
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
