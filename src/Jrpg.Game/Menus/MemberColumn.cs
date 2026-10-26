using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The narrow column of members beside a member page (decision 255): the members the page goes through, in its order,
/// which one it shows, and a window that scrolls only when the shown one would leave it.
/// </summary>
internal sealed class MemberColumn
{
    /// <summary>How many members show at once (placeholder).</summary>
    public const int VisibleCount = 8;

    private PartyMember[] _members = [];

    /// <summary>The members in the page's order; kept while the order stays the same.</summary>
    public IReadOnlyList<PartyMember> Members => _members;

    /// <summary>The shown member's place in <see cref="Members"/>.</summary>
    public int ShownIndex { get; private set; }

    /// <summary>The first member in view.</summary>
    public int FirstVisible { get; private set; }

    /// <summary>Takes the page's members if their order changed, marks the shown one and scrolls to it.</summary>
    /// <param name="members">The members the page goes through, in order.</param>
    /// <param name="shown">The member the page shows; one of <paramref name="members"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="shown"/> isn't among <paramref name="members"/>.</exception>
    public void Refresh(IReadOnlyList<PartyMember> members, PartyMember shown)
    {
        if (!IsSameOrder(members))
        {
            _members = [.. members];
        }

        ShownIndex = Array.IndexOf(_members, shown);
        if (ShownIndex < 0)
        {
            throw new ArgumentException($"Member '{shown.Id}' isn't in the page's list.", nameof(shown));
        }

        KeepShownInView();
    }

    // Runs every update a member page is open, so it compares in place rather than allocating.
    private bool IsSameOrder(IReadOnlyList<PartyMember> members)
    {
        if (members.Count != _members.Length)
        {
            return false;
        }

        for (var index = 0; index < _members.Length; index++)
        {
            if (!ReferenceEquals(members[index], _members[index]))
            {
                return false;
            }
        }

        return true;
    }

    private void KeepShownInView() => FirstVisible = ScrollWindow.KeepInView(FirstVisible, ShownIndex, _members.Length, VisibleCount);
}
