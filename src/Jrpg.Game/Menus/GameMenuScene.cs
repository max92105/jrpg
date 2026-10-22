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
/// The game menu over the map (legacy <c>GameMenuControl</c>, decision 81): tabs down the left, Confirm enters the
/// selected tab's page and Cancel comes back. Esc is Cancel and Menu at once, so inside a page it only goes back to the
/// tabs; on the tabs either closes the menu.
/// </summary>
internal sealed class GameMenuScene : IScene
{
    // Legacy GameMenuControl.OpenDuration; opacity only.
    private static readonly TimeSpan _openFade = TimeSpan.FromSeconds(0.15);

    // Legacy layout: the header, the body 40 px below it, the tabs 14 px apart, the page 24 px right of them, 40 px
    // from the right edge, and 32 px above the 64 px footer.
    private const int FooterHeight = 64;
    private const int BodyTop = MenuHeader.Height + 40;
    private const int TabPitch = NavTabShape.Height + 14;
    private const int PageLeft = NavTabShape.Width + 24;
    private static readonly Rectangle _pageArea = new(PageLeft, BodyTop, VirtualScreen.Width - 40 - PageLeft, VirtualScreen.Height - FooterHeight - 32 - BodyTop);
    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);

    // Legacy NavTabStyle: the selected tab's label slides 14 px right, over a 2 px bright line along its bottom.
    private const int SelectedLabelSlide = 14;
    private const int SelectionLineThickness = 2;

    // Legacy footer: the hint starts 40 px in.
    private const int FooterTextInset = 40;

    private readonly UiTextures _ui;
    private readonly MenuHeader _header;
    private readonly string _menuTitle;
    private readonly ImmutableArray<IMenuPage> _pages;
    private readonly ListCursor _tabs;
    private readonly SpriteFontBase _font;
    private readonly string[] _tabLabels;
    private readonly string _mainHint;
    private TimeSpan _sinceOpened;
    private bool _isInPage;

    /// <param name="text">Messages and fonts for the tab labels.</param>
    /// <param name="ui">The shared tab and gradient textures.</param>
    /// <param name="header">The header bar's title, location and play time.</param>
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
        _mainHint = text.Messages.Text(MessageIds.MenuHintMain);
    }

    /// <summary>The footer's line: the open page's hint, or the tabs' own.</summary>
    internal string FooterHint => _isInPage ? _pages[_tabs.Selected].Hint : _mainHint;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _sinceOpened += delta;
        _header.Update();
        return _isInPage ? UpdatePage(input) : UpdateTabs(input);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = (float)Math.Min(1, _sinceOpened / _openFade);
        DrawFrame(batch, textures, opacity);
        _header.Draw(batch, _isInPage ? _tabLabels[_tabs.Selected] : _menuTitle, opacity);
        for (var index = 0; index < _tabLabels.Length; index++)
        {
            DrawTab(batch, textures, index, opacity);
        }

        if (_isInPage)
        {
            _pages[_tabs.Selected].Draw(batch, textures, _pageArea, opacity);
        }

        DrawFooterHint(batch, opacity);
    }

    private void DrawFooterHint(SpriteBatch batch, float opacity)
    {
        var top = VirtualScreen.Height - FooterHeight + ((FooterHeight - _font.LineHeight) / 2);
        batch.DrawString(_font, FooterHint, new Vector2(FooterTextInset, top), UiTheme.TextSecondary * opacity);
    }

    // The picture with its edges darkened, between the header and footer bars.
    private void DrawFrame(SpriteBatch batch, TextureLibrary textures, float opacity)
    {
        var tint = Color.White * opacity;
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Menu), tint);
        batch.Draw(_ui.MenuScrim(textures), _screen, tint);

        var footerTop = VirtualScreen.Height - FooterHeight;
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, MenuHeader.Height), UiTheme.Bar * opacity);
        batch.Draw(textures.Pixel, new Rectangle(0, MenuHeader.Height - 1, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
        batch.Draw(textures.Pixel, new Rectangle(0, footerTop, VirtualScreen.Width, FooterHeight), UiTheme.Bar * opacity);
        batch.Draw(textures.Pixel, new Rectangle(0, footerTop, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
    }

    private void DrawTab(SpriteBatch batch, TextureLibrary textures, int index, float opacity)
    {
        var tint = Color.White * opacity;
        var top = BodyTop + (index * TabPitch);
        var isSelected = index == _tabs.Selected;
        batch.Draw(isSelected ? _ui.SelectedNavTab(textures) : _ui.NavTab(textures), new Vector2(0, top), tint);
        if (isSelected)
        {
            var line = new Rectangle(0, top + NavTabShape.Height - SelectionLineThickness, NavTabShape.ShoulderPixels, SelectionLineThickness);
            batch.Draw(_ui.SelectionLine(textures), line, tint);
        }

        var labelLeft = UiTheme.MenuTextInset + (isSelected ? SelectedLabelSlide : 0);
        var labelTop = top + ((NavTabShape.Height - _font.LineHeight) / 2f);
        batch.DrawString(_font, _tabLabels[index], new Vector2(labelLeft, labelTop), UiTheme.TextPrimary * opacity);
    }

    private MenuClosed? UpdateTabs(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel) || input.IsPressed(GameButton.Menu))
        {
            return new MenuClosed();
        }

        if (input.IsPressed(GameButton.Up))
        {
            _tabs.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _tabs.MoveDown();
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            _pages[_tabs.Selected].Open();
            _isInPage = true;
        }

        return null;
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
