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

    // Legacy layout: an 80 px header, the body 40 px below it, the 410 x 56 tabs 14 px apart, the page 24 px right of
    // them, 40 px from the right edge, and 32 px above the 64 px footer.
    private const int BodyTop = 80 + 40;
    private const int TabWidth = 410;
    private const int TabPitch = UiTheme.MenuRowHeight + 14;
    private const int PageLeft = TabWidth + 24;
    private static readonly Rectangle _pageArea = new(PageLeft, BodyTop, VirtualScreen.Width - 40 - PageLeft, VirtualScreen.Height - 64 - 32 - BodyTop);

    private readonly ImmutableArray<IMenuPage> _pages;
    private readonly ListCursor _tabs;
    private readonly SpriteFontBase _font;
    private readonly string[] _tabLabels;
    private TimeSpan _sinceOpened;
    private bool _isInPage;

    /// <param name="text">Messages and fonts for the tab labels.</param>
    /// <param name="pages">One per tab, top to bottom.</param>
    public GameMenuScene(ScreenText text, ImmutableArray<IMenuPage> pages)
    {
        _pages = pages;
        _tabs = new ListCursor(pages.Length, _ => true);
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _tabLabels = [.. pages.Select(page => text.Messages.Text(page.TitleMessageId))];
    }

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _sinceOpened += delta;
        return _isInPage ? UpdatePage(input) : UpdateTabs(input);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = (float)Math.Min(1, _sinceOpened / _openFade);
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.MenuBackground), Color.White * opacity);
        for (var index = 0; index < _tabLabels.Length; index++)
        {
            var color = index == _tabs.Selected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            var tabTop = BodyTop + (index * TabPitch);
            var position = new Vector2(UiTheme.MenuTextInset, tabTop + ((UiTheme.MenuRowHeight - _font.LineHeight) / 2f));
            batch.DrawString(_font, _tabLabels[index], position, color * opacity);
        }

        if (_isInPage)
        {
            _pages[_tabs.Selected].Draw(batch, textures, _pageArea, opacity);
        }
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
