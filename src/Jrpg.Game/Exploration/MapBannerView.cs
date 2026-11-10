using FontStashSharp;
using Jrpg.Core.Maps;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Draws the entry banner at the top right of the screen (decisions 527, 528), in the reference's look: the map's name
/// in the title serif over a hairline and, when it has one, its level as a small spaced-caps label beside a large
/// number, on a dark band fading out to the left, sliding in a little from the right while it fades in. The text is
/// measured once per shown map.
/// </summary>
internal sealed class MapBannerView
{
    // Under the name: the hairline's distance from it, its height, and the level row's distance from it (placeholders).
    private const int HairlineGap = 4;
    private const int HairlineHeight = 1;
    private const int LevelRowGap = 6;

    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _labelFont;
    private readonly SpriteFontBase _numberFont;
    private readonly UiTextures _ui;
    private readonly MapBanner _banner;
    private readonly MeasuredText _label;
    private MapDefinition? _measured;
    private MeasuredText _name;
    private MeasuredText? _number;
    private int _textWidth;
    private int _bandHeight;

    /// <param name="text">The fonts and the level label's word.</param>
    /// <param name="ui">Where the band's gradient comes from.</param>
    /// <param name="banner">What is shown, how opaque and how far slid in; the scene drives it.</param>
    public MapBannerView(ScreenText text, UiTextures ui, MapBanner banner)
    {
        _titleFont = text.Fonts.TitleFontAt(UiTheme.MapBannerTitlePixels);
        _labelFont = text.Fonts.FontAt(UiTheme.MapBannerLabelPixels);
        _numberFont = text.Fonts.FontAt(UiTheme.MapBannerNumberPixels);
        _ui = ui;
        _banner = banner;
        var caps = text.Messages.Text(MessageIds.StatusLevelShort).ToUpperInvariant();
        _label = new MeasuredText(caps, _labelFont.MeasureString(caps, characterSpacing: SheetRowLook.LabelSpacing).X);
    }

    /// <summary>The name as drawn; null while nothing is shown.</summary>
    internal string? NameLine => _measured is null ? null : _name.Text;

    /// <summary>The level's number as drawn; null while the shown map has no level or nothing is shown.</summary>
    internal string? LevelNumber => _number?.Text;

    /// <summary>
    /// The width the band and the hairline cover: the wider of the name and the level row; 0 while nothing is shown.
    /// </summary>
    internal int TextWidth => _textWidth;

    /// <summary>Measures the name and the level again only when the shown map changed.</summary>
    public void Update()
    {
        var shown = _banner.Shown;
        if (ReferenceEquals(shown, _measured))
        {
            return;
        }

        _measured = shown;
        if (shown is null)
        {
            (_name, _number, _textWidth, _bandHeight) = (default, null, 0, 0);
            return;
        }

        _name = MeasuredText.Of(_titleFont, shown.Settings.DisplayName);
        _number = shown.Settings.AreaLevel is { } level ? MeasuredText.Of(_numberFont, NumberText.Of(level)) : null;
        var levelRowWidth = _number is { } number ? _label.Width + UiTheme.MapBannerLabelGap + number.Width : 0;
        _textWidth = (int)Math.Ceiling(Math.Max(_name.Width, levelRowWidth));
        var levelRowHeight = _number is null ? 0 : LevelRowGap + _numberFont.LineHeight;
        _bandHeight = _titleFont.LineHeight + HairlineGap + HairlineHeight + levelRowHeight + (2 * UiTheme.MapBannerPadding.Y);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _banner.Opacity;
        if (_measured is null || opacity <= 0)
        {
            return;
        }

        // Whole pixels, so the text and the lines move together while sliding in.
        var slide = MathF.Round((1 - Easing.CubicOut(_banner.FadeInShare)) * UiTheme.MapBannerSlide);
        var right = VirtualScreen.Width - UiTheme.MapBannerRight + (int)slide;
        var left = right - _textWidth;
        // The band runs from the text's padding to the screen's right edge, past it while sliding in.
        var bandLeft = left - UiTheme.MapBannerPadding.X;
        var band = new Rectangle(bandLeft, UiTheme.MapBannerTop - UiTheme.MapBannerPadding.Y, VirtualScreen.Width + (int)slide - bandLeft, _bandHeight);
        batch.Draw(_ui.Gradient(textures, UiTheme.MapBannerBand), band, Color.White * opacity);
        batch.DrawString(_titleFont, _name.Text, new Vector2(right - _name.Width, UiTheme.MapBannerTop), UiTheme.TextPrimary * opacity);
        var hairlineTop = UiTheme.MapBannerTop + _titleFont.LineHeight + HairlineGap;
        batch.Draw(textures.Pixel, new Rectangle(left, hairlineTop, _textWidth, HairlineHeight), UiTheme.BarLine * opacity);
        if (_number is not { } number)
        {
            return;
        }

        var rowTop = hairlineTop + HairlineHeight + LevelRowGap;
        batch.DrawString(_numberFont, number.Text, new Vector2(right - number.Width, rowTop), UiTheme.TextPrimary * opacity);
        var labelLeft = right - number.Width - UiTheme.MapBannerLabelGap - _label.Width;
        var labelTop = rowTop + ((_numberFont.LineHeight - _labelFont.LineHeight) / 2f);
        batch.DrawString(_labelFont, _label.Text, new Vector2(labelLeft, labelTop), UiTheme.TextSecondary * opacity, characterSpacing: SheetRowLook.LabelSpacing);
    }
}
