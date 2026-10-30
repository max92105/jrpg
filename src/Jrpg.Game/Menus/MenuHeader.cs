using FontStashSharp;
using Jrpg.Core.Session;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The text on the menu's header bar: the page title on the left; the party's gold with its coin, the current map's
/// name and the ticking play time on the right. Text is measured only when it changes.
/// </summary>
internal sealed class MenuHeader
{
    /// <summary>The header bar's height (placeholder).</summary>
    public const int Height = 88;

    // Placeholder layout (decision 82): a 48 px margin, a 38 px title, 24 px gold, location and time, 40 px between
    // them.
    private const int SideMargin = 48;
    private const int TitleTextPixels = 38;
    private const int InfoTextPixels = 24;
    private const int LocationGap = 40;

    private readonly GameSession _session;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _infoFont;
    private readonly PlayTimeText _playTime = new();
    private readonly GoldAmountView _gold;
    private MeasuredText _shownPlayTime = new(string.Empty, 0);
    private MeasuredText _shownLocation = new(string.Empty, 0);
    private MeasuredText _shownGold = new(string.Empty, 0);
    private int _shownGoldAmount = -1;

    /// <param name="text">Fonts for the header.</param>
    /// <param name="ui">Where the coin icon comes from.</param>
    /// <param name="session">Where the gold, the play time and the current map come from.</param>
    public MenuHeader(ScreenText text, UiTextures ui, GameSession session)
    {
        _session = session;
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _infoFont = text.Fonts.FontAt(InfoTextPixels);
        _gold = new GoldAmountView(_infoFont, ui);
        Update();
    }

    /// <summary>The gold as the header shows it.</summary>
    internal string ShownGold => _shownGold.Text;

    /// <summary>
    /// Catches up with the play time (the flow adds to it every update, the menu included, decision 35) and the gold.
    /// </summary>
    public void Update()
    {
        var gold = _session.Party.Purse.Gold;
        if (gold != _shownGoldAmount)
        {
            _shownGoldAmount = gold;
            _shownGold = MeasuredText.Of(_infoFont, NumberText.Of(gold));
        }

        _playTime.Update(_session.PlayTime);
        if (!ReferenceEquals(_playTime.Text, _shownPlayTime.Text))
        {
            _shownPlayTime = MeasuredText.Of(_infoFont, _playTime.Text);
        }

        var location = _session.Exploration.CurrentMap.Settings.DisplayName;
        if (!ReferenceEquals(location, _shownLocation.Text))
        {
            _shownLocation = MeasuredText.Of(_infoFont, location);
        }
    }

    /// <param name="batch">Already begun; the bar itself is drawn by the menu.</param>
    /// <param name="textures">Where the coin icon comes from.</param>
    /// <param name="title">The page title, or the menu's own when no page is open.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, string title, float opacity)
    {
        batch.DrawString(_titleFont, title, new Vector2(SideMargin, CenteredTop(_titleFont)), UiTheme.TextPrimary * opacity);

        var playTimeLeft = VirtualScreen.Width - SideMargin - _shownPlayTime.Width;
        var infoTop = CenteredTop(_infoFont);
        batch.DrawString(_infoFont, _shownPlayTime.Text, new Vector2(playTimeLeft, infoTop), UiTheme.TextPrimary * opacity);
        var locationLeft = playTimeLeft - LocationGap - _shownLocation.Width;
        batch.DrawString(_infoFont, _shownLocation.Text, new Vector2(locationLeft, infoTop), UiTheme.TextSecondary * opacity);
        var goldLeft = locationLeft - LocationGap - _gold.WidthOf(_shownGold);
        _gold.Draw(batch, textures, new GoldAmountView.Place(_shownGold, new Vector2(goldLeft, infoTop), UiTheme.TextPrimary), opacity);
    }

    private static float CenteredTop(SpriteFontBase font) => (Height - font.LineHeight) / 2f;
}
