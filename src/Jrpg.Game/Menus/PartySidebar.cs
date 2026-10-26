using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Inventory's party sidebar (decision 266): one card per recruit, the active members in party order, then the
/// reserve (decision 146). Refreshed when the page opens and every update while it is open, it replaces only the cards
/// whose shown values changed, so a change (a potion, a new order on the Party page) shows at once and the view formats
/// again only what changed. It marks the card an item's pointer is on and scrolls so that card stays in view.
/// </summary>
internal sealed class PartySidebar
{
    /// <summary>
    /// How many cards show at once (placeholder): 5 cards of 132 px, 12 px apart, with the scroll cues and the reserve
    /// caption, fit the 848 px page (<see cref="PartySidebarView"/>).
    /// </summary>
    public const int VisibleCards = 5;

    private readonly PartyState _party;
    private readonly List<SidebarCard> _cards = [];

    public PartySidebar(PartyState party)
    {
        _party = party;
        RebuildAll();
    }

    public IReadOnlyList<SidebarCard> Cards => _cards;

    /// <summary>The member whose card carries the leader's badge.</summary>
    public PartyMember Leader => _party.Leader;

    /// <summary>The card the item's pointer is on; null while nothing is being aimed at.</summary>
    public int? PointedIndex { get; private set; }

    /// <summary>The first card in view; it moves only when the marked card would leave the view.</summary>
    public int FirstVisible { get; private set; }

    /// <summary>
    /// Rebuilds every card if the recruits' order or who is in the reserve changed, else only the cards whose values
    /// did; then marks <paramref name="pointed"/>'s card and scrolls to it.
    /// </summary>
    /// <param name="pointed">The member the item's pointer is on; null while nothing is being aimed at.</param>
    public void Refresh(PartyMember? pointed)
    {
        if (IsSameOrder())
        {
            ReplaceChangedCards();
        }
        else
        {
            RebuildAll();
        }

        PointedIndex = pointed is null ? null : IndexOf(pointed);
        KeepPointedInView();
    }

    private void ReplaceChangedCards()
    {
        for (var index = 0; index < _cards.Count; index++)
        {
            var card = _cards[index];
            var member = card.Member;
            if (card.Level != member.Level || card.Hp != member.Vitals.Hp)
            {
                _cards[index] = SidebarCard.Of(member, card.IsReserve);
            }
        }
    }

    // Runs every update, so it walks both lists in place rather than joining them.
    private bool IsSameOrder()
    {
        var activeCount = _party.ActiveMembers.Count;
        if (activeCount + _party.ReserveMembers.Count != _cards.Count)
        {
            return false;
        }

        for (var index = 0; index < _cards.Count; index++)
        {
            var isReserve = index >= activeCount;
            var member = isReserve ? _party.ReserveMembers[index - activeCount] : _party.ActiveMembers[index];
            if (!ReferenceEquals(member, _cards[index].Member) || isReserve != _cards[index].IsReserve)
            {
                return false;
            }
        }

        return true;
    }

    private void RebuildAll()
    {
        _cards.Clear();
        foreach (var member in _party.ActiveMembers)
        {
            _cards.Add(SidebarCard.Of(member, isReserve: false));
        }

        foreach (var member in _party.ReserveMembers)
        {
            _cards.Add(SidebarCard.Of(member, isReserve: true));
        }
    }

    private int? IndexOf(PartyMember member)
    {
        for (var index = 0; index < _cards.Count; index++)
        {
            if (ReferenceEquals(_cards[index].Member, member))
            {
                return index;
            }
        }

        return null;
    }

    private void KeepPointedInView() =>
        FirstVisible = PointedIndex is { } pointed
            ? ScrollWindow.KeepInView(FirstVisible, pointed, _cards.Count, VisibleCards)
            : ScrollWindow.Clamp(FirstVisible, _cards.Count, VisibleCards);
}
