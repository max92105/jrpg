using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws an <see cref="ItemTargetPicker"/> as the legacy use popup, in <c>DialogStyle</c>'s raised band over the dimmed
/// menu: the item's icon and "Use item", then every member's card (the Party page's), in two columns in member order.
/// Members the item can't be used on are faded; the chosen ones are drawn selected.
/// </summary>
internal sealed class ItemTargetPickerView
{
    // Legacy DialogStyle padding 32; Text.Heading 22 px with the 32 px item icon 12 px before it and 24 px below.
    private const int BandPadding = 32;
    private const int HeadingPixels = 22;
    private const int IconSize = 32;
    private const int IconGap = 12;
    private const int HeadingGap = 24;

    // Question 112's placeholder: two 560 px columns of cards, 16 px apart, centered.
    private const int Columns = 2;
    private const int CardWidth = 560;
    private const int ColumnGap = 16;
    private const float UnavailableOpacity = 0.4f;

    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly MemberCardView _cardView;
    private readonly SpriteFontBase _headingFont;
    private string _heading = string.Empty;
    private float _headingWidth;
    private string _iconFile = string.Empty;
    private MemberCardView.Card[] _cards = [];

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: portraits, icons, the card gradient and bars.</param>
    /// <param name="catalog">Class names, for the cards.</param>
    public ItemTargetPickerView(ScreenText text, UiTextures ui, ContentCatalog catalog)
    {
        _messages = text.Messages;
        _ui = ui;
        _cardView = new MemberCardView(text, ui, catalog);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
    }

    private int CardsHeight => (_cards.Length + Columns - 1) / Columns * MemberCardView.Height;

    /// <summary>Where the card at <paramref name="index"/> goes: left to right, then down, in member order.</summary>
    /// <param name="index">The member's index in the picker.</param>
    /// <param name="cardsTopLeft">The first card's top left corner.</param>
    internal static Rectangle CardArea(int index, Point cardsTopLeft) => new(
        cardsTopLeft.X + (index % Columns * (CardWidth + ColumnGap)),
        cardsTopLeft.Y + (index / Columns * MemberCardView.Height),
        CardWidth,
        MemberCardView.Height);

    /// <summary>Builds the text and cards once, when the picker opens.</summary>
    /// <param name="picker">The members and who can be chosen.</param>
    /// <param name="item">The item being used: its name and icon.</param>
    /// <param name="leader">Who gets the cards' Leader badge.</param>
    public void Show(ItemTargetPicker picker, ItemDefinition item, PartyMember leader)
    {
        _heading = _messages.Format(MessageIds.ItemUseTitle, new MessageArgument("item", item.Name));
        _headingWidth = _headingFont.MeasureString(_heading).X;
        _iconFile = item.IconFile;
        _cards = [.. picker.Members.Select(member => _cardView.CardOf(member, leader))];
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

        var cardsWidth = (Columns * CardWidth) + ((Columns - 1) * ColumnGap);
        var cardsTopLeft = new Point((VirtualScreen.Width - cardsWidth) / 2, band.Top + BandPadding + (int)_headingFont.LineHeight + HeadingGap);
        for (var index = 0; index < _cards.Length; index++)
        {
            var member = picker.Members[index];
            var cardOpacity = picker.IsCandidate(member) ? opacity : opacity * UnavailableOpacity;
            var place = new MemberCardView.Place(_cards[index], CardArea(index, cardsTopLeft), picker.IsSelected(member));
            _cardView.Draw(batch, textures, place, cardOpacity);
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
}
