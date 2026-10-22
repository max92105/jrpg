using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the party sidebar: the members' cards as the Party page draws them, on a dark panel that fades out toward the
/// page. A card is formatted again only when the sidebar replaced it.
/// </summary>
internal sealed class PartySidebarView
{
    // Cards 12 px apart (question 146 placeholder, the demo's PartySidebar).
    private const int CardGap = 12;

    private readonly PartySidebar _sidebar;
    private readonly MemberCardView _cardView;
    private readonly UiTextures _ui;
    private readonly List<(SidebarCard Source, MemberCardView.Card Card)> _cards = [];

    /// <param name="sidebar">The cards to draw.</param>
    /// <param name="cardView">Formats and draws one member's card.</param>
    /// <param name="ui">The panel's gradient.</param>
    public PartySidebarView(PartySidebar sidebar, MemberCardView cardView, UiTextures ui)
    {
        _sidebar = sidebar;
        _cardView = cardView;
        _ui = ui;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="column">The sidebar's column, in virtual-screen pixels.</param>
    /// <param name="opacity">0 to 1, while the menu or the page fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle column, float opacity)
    {
        FormatChangedCards();
        batch.Draw(_ui.Gradient(textures, UiTheme.SidebarPanel), column, Color.White * opacity);
        for (var index = 0; index < _cards.Count; index++)
        {
            var area = new Rectangle(column.X, column.Y + (index * (MemberCardView.Height + CardGap)), column.Width, MemberCardView.Height);
            _cardView.Draw(batch, textures, new MemberCardView.Place(_cards[index].Card, area, IsSelected: false), opacity);
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
}
