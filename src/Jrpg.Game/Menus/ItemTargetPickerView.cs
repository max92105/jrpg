using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws an <see cref="ItemTargetPicker"/> as the legacy use popup, in <c>DialogStyle</c>'s raised band over the dimmed
/// menu: the item's icon and "Use item", then every member side by side as a card (legacy <c>CharacterPortrait</c>), a
/// diamond portrait over the name and the HP and MP bars. Members the item can't be used on are faded; the chosen ones
/// glow.
/// </summary>
internal sealed class ItemTargetPickerView
{
    // Legacy DialogStyle padding 32; Text.Heading 22 px with the 32 px item icon 12 px before it and 24 px below.
    private const int BandPadding = 32;
    private const int HeadingPixels = 22;
    private const int NamePixels = 14;
    private const int IconSize = 32;
    private const int IconGap = 12;
    private const int HeadingGap = 24;

    // Each card: a 96 px diamond portrait (the reference UI's frame; legacy drew 48 px squares), the name 6 px under it,
    // then the 6 px HP and MP bars 2 px apart; cards 16 px apart.
    private const int CardWidth = 112;
    private const int CardGap = 16;
    private const int PortraitSize = 96;
    private const int NameGap = 6;
    private const int BarGap = 2;
    private const int BarHeight = 6;
    private const int SelectedGlow = 10;
    private const float UnavailableOpacity = 0.4f;

    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _nameFont;
    private string _heading = string.Empty;
    private float _headingWidth;
    private string _iconFile = string.Empty;
    private MemberCard[] _cards = [];

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: portraits, icons and bar gradients.</param>
    public ItemTargetPickerView(ScreenText text, UiTextures ui)
    {
        _messages = text.Messages;
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _nameFont = text.Fonts.FontAt(NamePixels);
    }

    private int CardsHeight => PortraitSize + NameGap + (int)_nameFont.LineHeight + NameGap + (2 * BarHeight) + BarGap;

    /// <summary>Builds the text and bars once, when the picker opens.</summary>
    /// <param name="picker">The members and who can be chosen.</param>
    /// <param name="itemName">The item being used.</param>
    /// <param name="iconFile">Its icon.</param>
    public void Show(ItemTargetPicker picker, string itemName, string iconFile)
    {
        _heading = _messages.Format(MessageIds.ItemUseTitle, new MessageArgument("item", itemName));
        _headingWidth = _headingFont.MeasureString(_heading).X;
        _iconFile = iconFile;
        _cards = [.. picker.Members.Select(member =>
        {
            var hp = new StatBar(_ui, UiTheme.HpBar);
            hp.Show(member.Vitals.Hp, member.Vitals.MaxHp);
            var mp = new StatBar(_ui, UiTheme.MpBar);
            mp.Show(member.Vitals.Mp, member.Vitals.MaxMp);
            return new MemberCard(MeasuredText.Of(_nameFont, member.Definition.Name), member.Definition.PortraitFile, hp, mp);
        })];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="picker">The same picker <see cref="Show"/> was given.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ItemTargetPicker picker, float opacity)
    {
        var bandHeight = (2 * BandPadding) + (int)_headingFont.LineHeight + HeadingGap + CardsHeight;
        var band = new Rectangle(0, (VirtualScreen.Height - bandHeight) / 2, VirtualScreen.Width, bandHeight);
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim * opacity);
        batch.Draw(textures.Pixel, band, UiTheme.RaisedSurface * opacity);
        batch.Draw(textures.Pixel, new Rectangle(0, band.Top, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
        batch.Draw(textures.Pixel, new Rectangle(0, band.Bottom - 1, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
        DrawHeading(batch, textures, band.Top + BandPadding, opacity);

        var cardsTop = band.Top + BandPadding + (int)_headingFont.LineHeight + HeadingGap;
        var rowWidth = (_cards.Length * CardWidth) + ((_cards.Length - 1) * CardGap);
        var left = (VirtualScreen.Width - rowWidth) / 2;
        for (var index = 0; index < _cards.Length; index++)
        {
            var member = picker.Members[index];
            var cardLeft = left + (index * (CardWidth + CardGap));
            var cardOpacity = picker.IsCandidate(member) ? opacity : opacity * UnavailableOpacity;
            DrawCard(batch, textures, new CardPlace(_cards[index], new Point(cardLeft, cardsTop), picker.IsSelected(member)), cardOpacity);
        }
    }

    // The icon then the title, centered together.
    private void DrawHeading(SpriteBatch batch, TextureLibrary textures, int top, float opacity)
    {
        var left = (VirtualScreen.Width - (IconSize + IconGap + _headingWidth)) / 2;
        var iconTop = top + ((_headingFont.LineHeight - IconSize) / 2);
        batch.Draw(_ui.Icon(textures, _iconFile, IconSize), new Vector2(left, iconTop), Color.White * opacity);
        batch.DrawString(_headingFont, _heading, new Vector2(left + IconSize + IconGap, top), UiTheme.TextPrimary * opacity);
    }

    private void DrawCard(SpriteBatch batch, TextureLibrary textures, CardPlace place, float opacity)
    {
        var card = place.Card;
        var center = new Vector2(place.TopLeft.X + (CardWidth / 2f), place.TopLeft.Y + (PortraitSize / 2f));
        if (place.IsSelected)
        {
            // A pale diamond behind the portrait, like the Status page's frame but wider: legacy's white glow.
            Diamond.WithDiagonal(center, PortraitSize + (2 * SelectedGlow)).Draw(batch, textures.Pixel, UiTheme.SelectionStroke * opacity);
        }

        var portrait = _ui.DiamondPortrait(textures, card.PortraitFile, PortraitSize);
        batch.Draw(portrait, new Vector2(center.X - (PortraitSize / 2f), place.TopLeft.Y), Color.White * opacity);

        var nameTop = place.TopLeft.Y + PortraitSize + NameGap;
        var nameColor = (place.IsSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary) * opacity;
        batch.DrawString(_nameFont, card.Name.Text, new Vector2(center.X - (card.Name.Width / 2), nameTop), nameColor);

        var barsTop = nameTop + (int)_nameFont.LineHeight + NameGap;
        var barLeft = place.TopLeft.X + ((CardWidth - PortraitSize) / 2);
        card.Hp.Draw(batch, textures, new Rectangle(barLeft, barsTop, PortraitSize, BarHeight), opacity);
        card.Mp.Draw(batch, textures, new Rectangle(barLeft, barsTop + BarHeight + BarGap, PortraitSize, BarHeight), opacity);
    }

    // One member's card, built when the picker opens.
    private sealed record MemberCard(MeasuredText Name, string PortraitFile, StatBar Hp, StatBar Mp);

    // Where a card goes this frame, and whether it's chosen.
    private readonly record struct CardPlace(MemberCard Card, Point TopLeft, bool IsSelected);
}
