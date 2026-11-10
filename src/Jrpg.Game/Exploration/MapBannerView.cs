using FontStashSharp;
using Jrpg.Core.Maps;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Draws the entry banner at the top right of the screen (decisions 527, 528): the map's name and, when it has one, its
/// area level under it, at the <see cref="MapBanner"/>'s opacity; the text is measured once per shown map.
/// </summary>
internal sealed class MapBannerView
{
    private readonly SpriteFontBase _font;
    private readonly UiMessages _messages;
    private readonly MapBanner _banner;
    private MapDefinition? _measured;
    private MeasuredText _name;
    private MeasuredText? _level;

    /// <param name="text">The fonts and the level line's words.</param>
    /// <param name="banner">What is shown and how opaque; the scene drives it.</param>
    public MapBannerView(ScreenText text, MapBanner banner)
    {
        _font = text.Fonts.FontAt(UiTheme.MapBannerTextPixels);
        _messages = text.Messages;
        _banner = banner;
    }

    /// <summary>The name as drawn; null while nothing is shown.</summary>
    internal string? NameLine => _measured is null ? null : _name.Text;

    /// <summary>The level line as drawn, "Lv 3"; null while the shown map has no level or nothing is shown.</summary>
    internal string? LevelLine => _level?.Text;

    /// <summary>Measures the name and the level line again only when the shown map changed.</summary>
    public void Update()
    {
        var shown = _banner.Shown;
        if (ReferenceEquals(shown, _measured))
        {
            return;
        }

        _measured = shown;
        _name = shown is null ? default : MeasuredText.Of(_font, shown.Settings.DisplayName);
        _level = shown?.Settings.AreaLevel is { } level
            ? MeasuredText.Of(_font, _messages.Format(MessageIds.MapBannerLevel, MessageArgument.Number("level", level)))
            : null;
    }

    public void Draw(SpriteBatch batch)
    {
        var opacity = _banner.Opacity;
        if (_measured is null || opacity <= 0)
        {
            return;
        }

        var color = UiTheme.TextPrimary * opacity;
        var right = VirtualScreen.Width - UiTheme.MapBannerRight;
        batch.DrawString(_font, _name.Text, new Vector2(right - _name.Width, UiTheme.MapBannerTop), color);
        if (_level is { } level)
        {
            batch.DrawString(_font, level.Text, new Vector2(right - level.Width, UiTheme.MapBannerTop + _font.LineHeight), color);
        }
    }
}
