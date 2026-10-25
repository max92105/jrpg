using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws one member as a card: the portrait, here in the reference UI's diamond, then
/// the name with a "Leader" badge, the level at the right, the class, and the HP bar with its value. The card
/// fills the width it's given, so the same card fits a Party column or the item target picker's.
/// </summary>
internal sealed class MemberCardView
{
    /// <summary>The card's height, with 12 px of padding above and below the portrait and text block.</summary>
    public const int Height = 120;

    // The card's layout (placeholders): 16 x 12 padding, an 80 px portrait, 16 px before the text, heading-size names,
    // caption-size level and class (8 px above the bars), 22 px bar rows of a 32 px label, a 6 px bar and a 100 px
    // value; a badge 10 px after the name with 4 px padding.
    private const int PaddingX = 16;
    private const int PaddingY = 12;
    private const int PortraitSize = 80;
    private const int TextGap = 16;
    private const int NamePixels = 22;
    private const int CaptionPixels = 14;
    private const int BadgePixels = 12;
    private const int ValuePixels = 15;
    private const int ClassGap = 8;
    private const int BarLabelColumn = 32;
    private const int BarValueColumn = 100;
    private const int BarHeight = 6;
    private const int BadgeGap = 10;
    private const int BadgePadding = 4;

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

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="place">The card, where it goes, and whether it's selected.</param>
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
        DrawBarRow(batch, textures, card.Vitals.Hp, new BarRow(_hpLabel, text.X, barsTop, text.Width), opacity);
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

    private void DrawBarRow(SpriteBatch batch, TextureLibrary textures, VitalMeter meter, BarRow row, float opacity)
    {
        batch.DrawString(_captionFont, row.Label, new Vector2(row.Left, row.Top), UiTheme.TextSecondary * opacity);
        var barTop = row.Top + ((int)_captionFont.LineHeight - BarHeight) / 2;
        var barWidth = row.Width - BarLabelColumn - BarValueColumn;
        meter.Bar.Draw(batch, textures, new Rectangle(row.Left + BarLabelColumn, barTop, barWidth, BarHeight), opacity);
        batch.DrawString(_valueFont, meter.Value.Text, new Vector2(row.Left + row.Width - meter.Value.Width, row.Top), UiTheme.TextPrimary * opacity);
    }

    // One bar row's label and where it goes.
    private readonly record struct BarRow(string Label, int Left, int Top, int Width);

    /// <summary>One member's card, formatted when what it shows changes.</summary>
    public sealed record Card(
        MeasuredText Name,
        bool IsLeader,
        MeasuredText Level,
        string ClassName,
        string PortraitFile,
        MemberVitalBars Vitals);

    /// <summary>Where a card goes this frame, and whether it's selected.</summary>
    public readonly record struct Place(Card Card, Rectangle Area, bool IsSelected);
}
