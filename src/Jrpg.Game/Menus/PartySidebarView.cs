using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the party sidebar: the members' cards as the Party page draws them, on a darker column. The reserve follows
/// under its own caption, dimmed (question 154); a small diamond at the top or bottom says cards are scrolled out of
/// view. While an item is aimed (decision 267) the cards it can go to are lit at full opacity, reserve or not, the
/// others faded, and the pointer is on the pointed card. A card is formatted again only when the sidebar replaced it.
/// </summary>
internal sealed class PartySidebarView
{
    // Placeholder layout: cards 12 px apart (question 146, the demo's PartySidebar), inside a 14 px band at the top and
    // the bottom that holds the 10 px scroll cues; the reserve's caption takes its label line and a card gap. With
    // PartySidebar.VisibleCards cards that is 14 + 38 + 5 x 132 + 4 x 12 + 14 = 774 of the page's 848 px.
    private const int CardGap = 12;
    private const int CueBand = 14;
    private const int CueDiagonal = 10;
    private const int CaptionHeight = SectionTitle.LineGap + CardGap;

    // While aiming, a card the item can't go to is faded (placeholder).
    private const float UnavailableOpacity = 0.4f;

    private readonly PartySidebar _sidebar;
    private readonly ColumnPanelView _panel = new(UiTheme.SideColumn);
    private readonly MemberCardView _cardView;
    private readonly SectionTitle _reserveTitle;
    private readonly List<(SidebarCard Source, MemberCardView.Card Card)> _cards = [];

    /// <param name="sidebar">The cards to draw.</param>
    /// <param name="cardView">Formats and draws one member's card.</param>
    /// <param name="text">The reserve caption's message and font.</param>
    /// <param name="ui">The reserve caption's hairline.</param>
    public PartySidebarView(PartySidebar sidebar, MemberCardView cardView, ScreenText text, UiTextures ui)
    {
        _sidebar = sidebar;
        _cardView = cardView;
        _reserveTitle = new SectionTitle(text, ui, MessageIds.PartyReserve);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="place">The sidebar's column, and the item being aimed, if any.</param>
    /// <param name="opacity">0 to 1, while the menu or the page fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var column = place.Column;
        FormatChangedCards();
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(column), opacity);
        var last = Math.Min(_cards.Count, _sidebar.FirstVisible + PartySidebar.VisibleCards);
        var top = column.Y + CueBand;
        for (var index = _sidebar.FirstVisible; index < last; index++)
        {
            if (IsFirstReserve(index))
            {
                _reserveTitle.Draw(batch, textures, new Rectangle(column.X, top, column.Width, 0), opacity);
                top += CaptionHeight;
            }

            DrawCard(batch, textures, new CardSlot(index, new Rectangle(column.X, top, column.Width, MemberCardView.Height), place.Picker), opacity);
            top += MemberCardView.Height + CardGap;
        }

        DrawScrollCues(batch, textures.Pixel, column, opacity);
    }

    private bool IsFirstReserve(int index) => _sidebar.Cards[index].IsReserve && (index == 0 || !_sidebar.Cards[index - 1].IsReserve);

    // Not aiming, the reserve is dimmed. Aiming, the item's candidates are lit at full opacity, reserve or not, the
    // others faded, and the pointed card gets the pointer.
    private void DrawCard(SpriteBatch batch, TextureLibrary textures, CardSlot slot, float opacity)
    {
        var card = _sidebar.Cards[slot.Index];
        if (slot.Picker is not { } picker)
        {
            var restingOpacity = card.IsReserve ? opacity * UiTheme.ReserveCardOpacity : opacity;
            _cardView.Draw(batch, textures, new MemberCardView.Place(_cards[slot.Index].Card, slot.Area, IsSelected: false), restingOpacity);
            return;
        }

        var aimedOpacity = picker.IsCandidate(card.Member) ? opacity : opacity * UnavailableOpacity;
        _cardView.Draw(batch, textures, new MemberCardView.Place(_cards[slot.Index].Card, slot.Area, picker.IsSelected(card.Member)), aimedOpacity);
        if (slot.Index == _sidebar.PointedIndex)
        {
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(slot.Area.X, slot.Area.Center.Y), opacity);
        }
    }

    private void DrawScrollCues(SpriteBatch batch, Texture2D pixel, Rectangle column, float opacity)
    {
        var color = UiTheme.TextSecondary * opacity;
        if (_sidebar.FirstVisible > 0)
        {
            Diamond.WithDiagonal(new Vector2(column.Center.X, column.Y + (CueBand / 2f)), CueDiagonal).Draw(batch, pixel, color);
        }

        if (_sidebar.FirstVisible + PartySidebar.VisibleCards < _cards.Count)
        {
            Diamond.WithDiagonal(new Vector2(column.Center.X, column.Bottom - (CueBand / 2f)), CueDiagonal).Draw(batch, pixel, color);
        }
    }

    private void FormatChangedCards()
    {
        var cards = _sidebar.Cards;
        if (cards.Count != _cards.Count)
        {
            _cards.Clear();
        }

        for (var index = 0; index < cards.Count; index++)
        {
            var source = cards[index];
            if (index == _cards.Count)
            {
                _cards.Add((source, _cardView.CardOf(source.Member, _sidebar.Leader)));
            }
            else if (!ReferenceEquals(_cards[index].Source, source))
            {
                _cards[index] = (source, _cardView.CardOf(source.Member, _sidebar.Leader));
            }
        }
    }

    // One card's index in the sidebar, where it's drawn, and the item being aimed, if any.
    private readonly record struct CardSlot(int Index, Rectangle Area, ItemTargetPicker? Picker);

    /// <summary>Where the sidebar goes this frame.</summary>
    /// <param name="Column">The sidebar's column, in virtual-screen pixels.</param>
    /// <param name="Picker">Who the item being used can go to; null while no item is aimed.</param>
    public readonly record struct Place(Rectangle Column, ItemTargetPicker? Picker);
}
