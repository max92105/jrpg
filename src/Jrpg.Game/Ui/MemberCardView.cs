using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws one member as a card: the portrait, here in the reference UI's diamond, then the name with a "Leader" badge,
/// the level at the right, the class, and the HP bar with its value, or the HP an aimed item would leave. The card
/// fills the width it's given, so the same card fits a Party column or the Inventory's sidebar.
/// </summary>
internal sealed class MemberCardView
{
    /// <summary>The card's height, with 14 px of padding above and below the portrait and text block.</summary>
    public const int Height = 132;

    // The card's layout (placeholders): 16 x 14 padding, a 96 px portrait, 18 px before the text, heading-size names,
    // caption-size level and class (8 px above the bars), bar rows of a 40 px label, a 6 px bar and a 140 px large
    // value; a badge 10 px after the name with 4 px padding.
    private const int PaddingX = 16;
    private const int PaddingY = 14;
    private const int PortraitSize = 96;
    private const int TextGap = 18;
    private const int NamePixels = 28;
    private const int CaptionPixels = 18;
    private const int BadgePixels = 14;
    private const int ValuePixels = 24;
    private const int ClassGap = 8;
    private const int BarLabelColumn = 40;
    private const int BarValueColumn = 140;
    private const int BarHeight = 6;
    private const int BadgeGap = 10;
    private const int BadgePadding = 4;

    // An HP preview's mark between the two values: Equipment's 6 px preview diamond, its center 12 px from each value.
    private const int PreviewMarkSide = 6;
    private const int PreviewMarkSpacing = 12;

    private readonly IReadOnlyDictionary<string, ClassDefinition> _classes;
    private readonly ScreenText _text;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _captionFont;
    private readonly SpriteFontBase _badgeFont;
    private readonly SpriteFontBase _valueFont;
    private readonly MeasuredText _leaderBadge;
    private readonly string _hpLabel;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: portraits, the card gradient, the bars.</param>
    /// <param name="catalog">Class names.</param>
    public MemberCardView(ScreenText text, UiTextures ui, ContentCatalog catalog)
    {
        _classes = catalog.Classes;
        _text = text;
        _messages = text.Messages;
        _ui = ui;
        _nameFont = text.Fonts.FontAt(NamePixels);
        _captionFont = text.Fonts.FontAt(CaptionPixels);
        _badgeFont = text.Fonts.FontAt(BadgePixels);
        _valueFont = text.Fonts.FontAt(ValuePixels);
        var badge = text.Messages.Text(MessageIds.PartyLeader);
        _leaderBadge = MeasuredText.Of(_badgeFont, badge);
        _hpLabel = text.Messages.Text(MessageIds.StatHp);
    }

    /// <summary>Formats the member's card once, when the party changes, so drawing formats nothing.</summary>
    public Card CardOf(PartyMember member, PartyMember leader)
    {
        var vitals = new MemberVitalBars(_text, _ui, ValuePixels);
        vitals.Show(member.Vitals);
        var level = _messages.Format(MessageIds.PartyLevel, MessageArgument.Number("level", member.Level));
        return new Card(
            MeasuredText.Of(_nameFont, member.Definition.Name),
            member == leader,
            MeasuredText.Of(_captionFont, level),
            _classes[member.Definition.ClassId].Name,
            member.Definition.PortraitFile,
            vitals);
    }

    /// <summary>Measures an HP preview in the card's value font, once, when it changes.</summary>
    /// <param name="hp">The member's HP now.</param>
    /// <param name="hpAfter">The HP an item would leave, from Core.</param>
    public HpPreview HpPreviewOf(int hp, int hpAfter) => new(
        MeasuredText.Of(_valueFont, NumberText.Of(hp)),
        MeasuredText.Of(_valueFont, NumberText.Of(hpAfter)),
        UiTheme.PreviewColorOf(hpAfter - hp));

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="place">The card, where it goes, whether it's selected, and its HP preview, if any.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var area = place.Area;
        var card = place.Card;
        batch.Draw(_ui.Gradient(textures, UiTheme.Card), area, Color.White * opacity);
        batch.Draw(textures.Pixel, new Rectangle(area.X, area.Bottom - 1, area.Width, 1), UiTheme.Stroke * opacity);
        if (place.IsSelected)
        {
            _ui.DrawSelectedRow(batch, textures, area, opacity);
        }

        var portrait = _ui.DiamondPortrait(textures, card.PortraitFile, PortraitSize);
        batch.Draw(portrait, new Vector2(area.X + PaddingX, area.Y + ((Height - PortraitSize) / 2)), Color.White * opacity);

        var text = new Rectangle(area.X + PaddingX + PortraitSize + TextGap, area.Y + PaddingY, 0, 0);
        text.Width = area.Right - PaddingX - text.X;
        DrawHeadline(batch, textures, card, text, opacity);
        var classTop = text.Y + _nameFont.LineHeight;
        batch.DrawString(_captionFont, card.ClassName, new Vector2(text.X, classTop), UiTheme.TextSecondary * opacity);
        var barsTop = (int)(classTop + _captionFont.LineHeight + ClassGap);
        DrawBarRow(batch, textures, new BarRow(_hpLabel, text.X, barsTop, text.Width), new BarValue(card.Vitals.Hp, place.Preview), opacity);
    }

    // The name, the leader's badge after it, and the level at the right.
    private void DrawHeadline(SpriteBatch batch, TextureLibrary textures, Card card, Rectangle text, float opacity)
    {
        batch.DrawString(_nameFont, card.Name.Text, new Vector2(text.X, text.Y), UiTheme.TextPrimary * opacity);
        var levelTop = text.Y + ((_nameFont.LineHeight - _captionFont.LineHeight) / 2);
        batch.DrawString(_captionFont, card.Level.Text, new Vector2(text.Right - card.Level.Width, levelTop), UiTheme.TextSecondary * opacity);
        if (!card.IsLeader)
        {
            return;
        }

        // A small label in a thin box beside the name.
        var badgeWidth = (int)_leaderBadge.Width + (2 * BadgePadding);
        var badgeHeight = (int)_badgeFont.LineHeight + 2;
        var badge = new Rectangle((int)(text.X + card.Name.Width + BadgeGap), (int)(text.Y + ((_nameFont.LineHeight - badgeHeight) / 2)), badgeWidth, badgeHeight);
        Outline.Draw(batch, textures.Pixel, badge, UiTheme.TextSecondary * opacity);
        batch.DrawString(_badgeFont, _leaderBadge.Text, new Vector2(badge.X + BadgePadding, badge.Y + 1), UiTheme.TextSecondary * opacity);
    }

    // The label and the bar centered on the value's line, the large value beside them: "now / max", or with a preview
    // "now ◆ after" in the same column, so the bar keeps its length.
    private void DrawBarRow(SpriteBatch batch, TextureLibrary textures, BarRow row, BarValue value, float opacity)
    {
        var labelTop = row.Top + ((_valueFont.LineHeight - _captionFont.LineHeight) / 2);
        batch.DrawString(_captionFont, row.Label, new Vector2(row.Left, labelTop), UiTheme.TextSecondary * opacity);
        var barTop = row.Top + ((int)_valueFont.LineHeight - BarHeight) / 2;
        var barWidth = row.Width - BarLabelColumn - BarValueColumn;
        value.Meter.Bar.Draw(batch, textures, new Rectangle(row.Left + BarLabelColumn, barTop, barWidth, BarHeight), opacity);
        var valueRight = new Vector2(row.Left + row.Width, row.Top);
        if (value.Preview is { } preview)
        {
            DrawHpPreview(batch, textures.Pixel, preview, valueRight, opacity);
            return;
        }

        batch.DrawString(_valueFont, value.Meter.Value.Text, valueRight - new Vector2(value.Meter.Value.Width, 0), UiTheme.TextPrimary * opacity);
    }

    // Right-aligned from the value column's right edge: the after value, the mark, then the current HP.
    private void DrawHpPreview(SpriteBatch batch, Texture2D pixel, HpPreview preview, Vector2 right, float opacity)
    {
        var after = right - new Vector2(preview.After.Width, 0);
        batch.DrawString(_valueFont, preview.After.Text, after, preview.Color * opacity);
        var markCenter = new Vector2(after.X - PreviewMarkSpacing, after.Y + (_valueFont.LineHeight / 2f));
        new Diamond(markCenter, PreviewMarkSide).Draw(batch, pixel, UiTheme.TextSecondary * opacity);
        var now = new Vector2(markCenter.X - PreviewMarkSpacing - preview.Now.Width, right.Y);
        batch.DrawString(_valueFont, preview.Now.Text, now, UiTheme.TextPrimary * opacity);
    }

    // One bar row's label and where it goes.
    private readonly record struct BarRow(string Label, int Left, int Top, int Width);

    // One bar row's meter, and the preview that replaces its value, if any.
    private readonly record struct BarValue(VitalMeter Meter, HpPreview? Preview);

    /// <summary>One member's card, formatted when what it shows changes.</summary>
    public sealed record Card(
        MeasuredText Name,
        bool IsLeader,
        MeasuredText Level,
        string ClassName,
        string PortraitFile,
        MemberVitalBars Vitals);

    /// <summary>
    /// Where a card goes this frame, whether it's selected, and the HP preview shown in place of its HP value; null
    /// when no item is aimed at the card.
    /// </summary>
    public readonly record struct Place(Card Card, Rectangle Area, bool IsSelected, HpPreview? Preview);
}
