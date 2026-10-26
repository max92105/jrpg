using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's party sidebar: one card per recruit, the active members in party order, then the reserve (decision 146).
/// Refreshed every update a page is open, it replaces only the cards whose shown values changed, so a page's own changes
/// (a potion, a new order) show at once and the view formats again only what changed. It marks the member a page shows
/// and scrolls so that card stays in view.
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

    /// <summary>The card of the member the open page shows; null on a page that shows no one member.</summary>
    public int? ShownIndex { get; private set; }

    /// <summary>The first card in view; it moves only when the marked card would leave the view.</summary>
    public int FirstVisible { get; private set; }

    /// <summary>
    /// Rebuilds every card if the recruits' order or who is in the reserve changed, else only the cards whose values did;
    /// then marks <paramref name="shown"/>'s card and scrolls to it.
    /// </summary>
    /// <param name="shown">The member the open page shows; null on other pages.</param>
    public void Refresh(PartyMember? shown)
    {
        if (IsSameOrder())
        {
            ReplaceChangedCards();
        }
        else
        {
            RebuildAll();
        }

        ShownIndex = shown is null ? null : IndexOf(shown);
        KeepShownInView();
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

    private void KeepShownInView()
    {
        FirstVisible = Math.Min(FirstVisible, Math.Max(0, _cards.Count - VisibleCards));
        if (ShownIndex is not { } shown)
        {
            return;
        }

        if (shown < FirstVisible)
        {
            FirstVisible = shown;
        }
        else if (shown >= FirstVisible + VisibleCards)
        {
            FirstVisible = shown - VisibleCards + 1;
        }
    }
}
