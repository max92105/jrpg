using FontStashSharp;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Trade;

/// <summary>
/// Draws the Shop screen in the menu's frame, as the Forge: the map dimmed behind it, the header bar with the shop's
/// name at the left and the party's gold with its coin at the right (decision 380), the tabs and the shown list in the
/// list column, the detail column, and the footer with its hint. Everything fades with the Shop (decision 370). The
/// gold is measured when it changes.
/// </summary>
internal sealed class ShopView
{
    /// <summary>The rows' and lines' text size, the menu pages' body size.</summary>
    public const int BodyPixels = 26;

    // The menu header's title size, its side margin and its info size; the footer's hint 40 px in, as the menu's.
    private const int TitleTextPixels = 38;
    private const int SideMargin = 48;
    private const int InfoTextPixels = 24;
    private const int FooterTextInset = 40;

    // The Forge's columns at 1920 (placeholder): the list from x 40 to 1000, the detail from 1040 to 1880, both the
    // menu's body height.
    private const int ListLeft = 40;
    private const int ListWidth = 960;
    private const int DetailLeft = 1040;
    private const int DetailWidth = 840;

    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);
    private static readonly Rectangle _listArea = new(ListLeft, MenuLayout.BodyTop, ListWidth, MenuLayout.BodyHeight);
    private static readonly Rectangle _detailArea = new(DetailLeft, MenuLayout.BodyTop, DetailWidth, MenuLayout.BodyHeight);

    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private readonly ShopListView _list;
    private readonly GoldAmountView _gold;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _infoFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _name;
    private readonly UiMessages _messages;
    private string _hint = string.Empty;
    private InputDevice? _hintDevice;
    private MeasuredText _shownGold = new(string.Empty, 0);
    private int _shownGoldAmount = -1;

    /// <param name="text">The fonts and the footer's line.</param>
    /// <param name="ui">The shared textures: the coin, the columns.</param>
    /// <param name="name">The shop's name, the header's title.</param>
    public ShopView(ScreenText text, UiTextures ui, string name)
    {
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _infoFont = text.Fonts.FontAt(InfoTextPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _gold = new GoldAmountView(_infoFont, ui);
        _list = new ShopListView(text, ui, _bodyFont);
        _name = name;
        _messages = text.Messages;
    }

    /// <summary>Names the tab buttons in the footer for the device in use, formatted only when it changes.</summary>
    public void ShowDevice(InputDevice device)
    {
        if (device == _hintDevice)
        {
            return;
        }

        _hintDevice = device;
        _hint = _messages.Format(
            MessageIds.ShopHint,
            new MessageArgument("previous", ButtonLabels.Of(GameButton.PreviousTab, device)),
            new MessageArgument("next", ButtonLabels.Of(GameButton.NextTab, device)));
    }

    /// <summary>Formats the gold only when the purse changed, so drawing allocates nothing.</summary>
    public void ShowGold(int gold)
    {
        if (gold == _shownGoldAmount)
        {
            return;
        }

        _shownGoldAmount = gold;
        _shownGold = MeasuredText.Of(_infoFont, NumberText.Of(gold));
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel, the coin and the gradients come from.</param>
    /// <param name="place">The tab shown, where the cursor is, and the shown tab's rows.</param>
    /// <param name="opacity">The Shop's fade, 0 for clear to 1 for opaque.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ShopListView.Place place, float opacity)
    {
        batch.Draw(textures.Pixel, _screen, UiTheme.Scrim * opacity);
        HeaderBar.Draw(batch, textures.Pixel, MenuHeader.Height, opacity);
        FooterBar.Draw(batch, textures.Pixel, MenuLayout.FooterHeight, opacity);
        batch.DrawString(_titleFont, _name, new Vector2(SideMargin, CenteredTop(_titleFont)), UiTheme.TextPrimary * opacity);
        var goldLeft = VirtualScreen.Width - SideMargin - _gold.WidthOf(_shownGold);
        _gold.Draw(batch, textures, new GoldAmountView.Place(_shownGold, new Vector2(goldLeft, CenteredTop(_infoFont)), UiTheme.TextPrimary), opacity);
        batch.DrawString(_bodyFont, _hint, new Vector2(FooterTextInset, FooterBar.TextTop(MenuLayout.FooterHeight, _bodyFont.LineHeight)), UiTheme.TextSecondary * opacity);

        _list.Draw(batch, textures, place, _listArea, opacity);
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(_detailArea), opacity);
    }

    private static float CenteredTop(SpriteFontBase font) => (MenuHeader.Height - font.LineHeight) / 2f;
}
