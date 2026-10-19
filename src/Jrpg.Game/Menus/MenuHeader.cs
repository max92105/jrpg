using FontStashSharp;
using Jrpg.Core.Session;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The text on the menu's header bar, as legacy <c>MenuHeader</c>: the page title on the left; the current map's name
/// and the ticking play time on the right. Text is measured only when it changes.
/// </summary>
internal sealed class MenuHeader
{
    /// <summary>The header bar's height (legacy <c>GameMenuControl</c>'s first row).</summary>
    public const int Height = 80;

    // Legacy DockPanel margin 48; Text.Title 28 px; Text.Secondary and Text.Number 18 px; 40 px between location and time.
    private const int SideMargin = 48;
    private const int TitleTextPixels = 28;
    private const int InfoTextPixels = 18;
    private const int LocationGap = 40;

    private readonly GameSession _session;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _infoFont;
    private readonly PlayTimeText _playTime = new();
    private string _measuredPlayTime = string.Empty;
    private float _playTimeWidth;
    private string _measuredLocation = string.Empty;
    private float _locationWidth;

    /// <param name="text">Fonts for the header.</param>
    /// <param name="session">Where the play time and the current map come from.</param>
    public MenuHeader(ScreenText text, GameSession session)
    {
        _session = session;
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _infoFont = text.Fonts.FontAt(InfoTextPixels);
        Update();
    }

    /// <summary>Catches up with the play time; the flow adds to it every update, the menu included (decision 35).</summary>
    public void Update()
    {
        _playTime.Update(_session.PlayTime);
        if (!ReferenceEquals(_playTime.Text, _measuredPlayTime))
        {
            _measuredPlayTime = _playTime.Text;
            _playTimeWidth = _infoFont.MeasureString(_measuredPlayTime).X;
        }

        var location = _session.Exploration.CurrentMap.Settings.DisplayName;
        if (!ReferenceEquals(location, _measuredLocation))
        {
            _measuredLocation = location;
            _locationWidth = _infoFont.MeasureString(location).X;
        }
    }

    /// <param name="batch">Already begun; the bar itself is drawn by the menu.</param>
    /// <param name="title">The page title, or the menu's own when no page is open.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, string title, float opacity)
    {
        batch.DrawString(_titleFont, title, new Vector2(SideMargin, CenteredTop(_titleFont)), UiTheme.TextPrimary * opacity);

        var playTimeLeft = VirtualScreen.Width - SideMargin - _playTimeWidth;
        var infoTop = CenteredTop(_infoFont);
        batch.DrawString(_infoFont, _playTime.Text, new Vector2(playTimeLeft, infoTop), UiTheme.TextPrimary * opacity);
        var locationLeft = playTimeLeft - LocationGap - _locationWidth;
        batch.DrawString(_infoFont, _measuredLocation, new Vector2(locationLeft, infoTop), UiTheme.TextSecondary * opacity);
    }

    private static float CenteredTop(SpriteFontBase font) => (Height - font.LineHeight) / 2f;
}
