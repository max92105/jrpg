using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the party sidebar: the members' cards as the Party page draws them, on a dark panel that fades out toward the
/// page. The shown member's card is lit and pointed at; the reserve follows under its own caption, dimmed (question
/// 154); a small diamond at the top or bottom says cards are scrolled out of view. A card is formatted again only when
/// the sidebar replaced it.
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

    private readonly PartySidebar _sidebar;
    private readonly MemberCardView _cardView;
    private readonly UiTextures _ui;
    private readonly SectionTitle _reserveTitle;
    private readonly List<(SidebarCard Source, MemberCardView.Card Card)> _cards = [];

    /// <param name="sidebar">The cards to draw.</param>
    /// <param name="cardView">Formats and draws one member's card.</param>
    /// <param name="text">The reserve caption's message and font.</param>
    /// <param name="ui">The panel's gradient and the caption's hairline.</param>
    public PartySidebarView(PartySidebar sidebar, MemberCardView cardView, ScreenText text, UiTextures ui)
    {
        _sidebar = sidebar;
        _cardView = cardView;
        _ui = ui;
        _reserveTitle = new SectionTitle(text, ui, MessageIds.PartyReserve);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="place">The sidebar's column, and whether it points at the shown card.</param>
    /// <param name="opacity">0 to 1, while the menu or the page fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var column = place.Column;
        FormatChangedCards();
        batch.Draw(_ui.Gradient(textures, UiTheme.SidebarPanel), column, Color.White * opacity);
        var last = Math.Min(_cards.Count, _sidebar.FirstVisible + PartySidebar.VisibleCards);
        var top = column.Y + CueBand;
        for (var index = _sidebar.FirstVisible; index < last; index++)
        {
            if (IsFirstReserve(index))
            {
                _reserveTitle.Draw(batch, textures, new Rectangle(column.X, top, column.Width, 0), opacity);
                top += CaptionHeight;
            }

            DrawCard(batch, textures, new CardSlot(index, new Rectangle(column.X, top, column.Width, MemberCardView.Height), place.PointsAtShown), opacity);
            top += MemberCardView.Height + CardGap;
        }

        DrawScrollCues(batch, textures.Pixel, column, opacity);
    }

    private bool IsFirstReserve(int index) => _sidebar.Cards[index].IsReserve && (index == 0 || !_sidebar.Cards[index - 1].IsReserve);

    // The shown member's card is lit, pointed at unless the page points at its own row, and never dimmed, even in the
    // reserve.
    private void DrawCard(SpriteBatch batch, TextureLibrary textures, CardSlot slot, float opacity)
    {
        var isShown = slot.Index == _sidebar.ShownIndex;
        var cardOpacity = _sidebar.Cards[slot.Index].IsReserve && !isShown ? opacity * UiTheme.ReserveCardOpacity : opacity;
        _cardView.Draw(batch, textures, new MemberCardView.Place(_cards[slot.Index].Card, slot.Area, isShown), cardOpacity);
        if (isShown && slot.PointsAtShown)
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

    // One card's index in the sidebar, where it's drawn, and whether the shown card gets the pointer.
    private readonly record struct CardSlot(int Index, Rectangle Area, bool PointsAtShown);

    /// <summary>Where the sidebar goes this frame.</summary>
    /// <param name="Column">The sidebar's column, in virtual-screen pixels.</param>
    /// <param name="PointsAtShown">
    /// False on a page that points at a row of its own, so only one pointer shows (decision 252).
    /// </param>
    public readonly record struct Place(Rectangle Column, bool PointsAtShown);
}
