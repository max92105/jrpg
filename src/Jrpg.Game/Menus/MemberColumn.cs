using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The member column on a member page (decision 255): the members the page goes through, in its order, and which one it
/// shows, scrolled so the shown one stays in view. Refreshed every update a member page is open.
/// </summary>
internal sealed class MemberColumn
{
    /// <summary>How many members show at once (placeholder).</summary>
    public const int VisibleCount = 8;

    private readonly List<PartyMember> _members = [];

    public IReadOnlyList<PartyMember> Members => _members;

    /// <summary>The shown member's place in <see cref="Members"/>.</summary>
    public int ShownIndex { get; private set; }

    /// <summary>The first member in view; it moves only when the shown one would leave the view.</summary>
    public int FirstVisible { get; private set; }

    /// <summary>
    /// Takes <paramref name="members"/> if their order changed, keeping the scroll otherwise; then marks
    /// <paramref name="shown"/> and scrolls to it.
    /// </summary>
    /// <param name="members">The members the open page goes through, in its order.</param>
    /// <param name="shown">The one it shows; one of <paramref name="members"/>.</param>
    public void Refresh(IReadOnlyList<PartyMember> members, PartyMember shown)
    {
        if (!IsSameOrder(members))
        {
            _members.Clear();
            _members.AddRange(members);
            FirstVisible = 0;
        }

        ShownIndex = _members.IndexOf(shown);
        FirstVisible = ListScroll.KeepInView(FirstVisible, ShownIndex, VisibleCount);
    }

    // Runs every update, so it compares in place rather than with LINQ.
    private bool IsSameOrder(IReadOnlyList<PartyMember> members)
    {
        if (members.Count != _members.Count)
        {
            return false;
        }

        for (var index = 0; index < members.Count; index++)
        {
            if (!ReferenceEquals(members[index], _members[index]))
            {
                return false;
            }
        }

        return true;
    }
}
