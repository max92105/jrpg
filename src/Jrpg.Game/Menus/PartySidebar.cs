using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's party sidebar: one card per active member, in party order. Refreshed every update a page is open, it
/// replaces only the cards whose shown values changed, so a page's own changes (a potion, a new order) show at once and
/// the view formats again only what changed.
/// </summary>
internal sealed class PartySidebar
{
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

    /// <summary>Rebuilds every card if the active members or their order changed, else only the cards whose values did.</summary>
    public void Refresh()
    {
        var active = _party.ActiveMembers;
        if (!IsSameOrder(active))
        {
            RebuildAll();
            return;
        }

        for (var index = 0; index < active.Count; index++)
        {
            var card = _cards[index];
            var member = active[index];
            if (card.Level != member.Level || card.Hp != member.Vitals.Hp || card.Mp != member.Vitals.Mp)
            {
                _cards[index] = SidebarCard.Of(member);
            }
        }
    }

    private bool IsSameOrder(IReadOnlyList<PartyMember> active)
    {
        if (active.Count != _cards.Count)
        {
            return false;
        }

        for (var index = 0; index < active.Count; index++)
        {
            if (!ReferenceEquals(active[index], _cards[index].Member))
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
            _cards.Add(SidebarCard.Of(member));
        }
    }
}
