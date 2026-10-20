namespace Jrpg.Core.Party;

/// <summary>
/// Recruited members, the ordered active party and the reserve (recruited members not in the active party, in
/// recruitment order). The active party never exceeds <see cref="PartyRules.MaxActiveMembers"/>, and the leader (the
/// first member of the starting active order) is always first in it.
/// </summary>
public sealed class PartyState
{
    private readonly PartyRules _rules;
    private readonly List<PartyMember> _activeMembers;
    private readonly List<PartyMember> _reserveMembers = [];

    public PartyState(IReadOnlyList<PartyMember> members, IReadOnlyList<PartyMember> activeOrder, PartyRules rules)
    {
        if (activeOrder.Count == 0 || activeOrder.Count > rules.MaxActiveMembers)
        {
            throw new ArgumentException(
                $"The active party needs 1 to {rules.MaxActiveMembers} members (was {activeOrder.Count}).", nameof(activeOrder));
        }

        if (activeOrder.Distinct().Count() != activeOrder.Count || activeOrder.Except(members).Any())
        {
            throw new ArgumentException("Active members must be distinct recruited members.", nameof(activeOrder));
        }

        _rules = rules;
        Members = [.. members];
        _activeMembers = [.. activeOrder];
        Leader = activeOrder[0];
        RebuildReserve();
    }

    public IReadOnlyList<PartyMember> Members { get; }

    public IReadOnlyList<PartyMember> ActiveMembers => _activeMembers;

    public IReadOnlyList<PartyMember> ReserveMembers => _reserveMembers;

    public PartyMember Leader { get; }

    public bool IsFull => _activeMembers.Count >= _rules.MaxActiveMembers;

    /// <exception cref="ArgumentException">No recruited member has that id.</exception>
    public PartyMember MemberById(string id) =>
        Members.FirstOrDefault(member => member.Id == id)
        ?? throw new ArgumentException($"'{id}' is not a recruited member.", nameof(id));

    /// <returns>False if the party is full or the member is already active.</returns>
    public bool TryAddToActive(PartyMember member)
    {
        ThrowIfNotRecruited(member);
        if (IsFull || _activeMembers.Contains(member))
        {
            return false;
        }

        _activeMembers.Add(member);
        RebuildReserve();
        return true;
    }

    /// <summary>
    /// Whether the member may leave the active party: they are active, aren't the leader (decision 107), and another
    /// active member is standing, so the active party is never left entirely fallen.
    /// </summary>
    /// <exception cref="ArgumentException">The member isn't recruited.</exception>
    public bool CanMoveToReserve(PartyMember member)
    {
        ThrowIfNotRecruited(member);
        return member != Leader
            && _activeMembers.Contains(member)
            && _activeMembers.Exists(other => other != member && other.Vitals.IsAlive);
    }

    /// <summary>
    /// Whether the member may swap places with another active member: they are active and aren't the leader, who
    /// always stays first. Unlike <see cref="CanMoveToReserve"/>, nobody needs to be standing: reordering never changes
    /// who is active.
    /// </summary>
    /// <exception cref="ArgumentException">The member isn't recruited.</exception>
    public bool CanChangePlace(PartyMember member)
    {
        ThrowIfNotRecruited(member);
        return member != Leader && _activeMembers.Contains(member);
    }

    /// <returns>False when <see cref="CanMoveToReserve"/> refuses the member.</returns>
    public bool TryMoveToReserve(PartyMember member)
    {
        if (!CanMoveToReserve(member) || !_activeMembers.Remove(member))
        {
            return false;
        }

        RebuildReserve();
        return true;
    }

    /// <exception cref="ArgumentException">
    /// The new order has different members, or moves a member who can't change place (the leader isn't first).
    /// </exception>
    public void Reorder(IReadOnlyList<PartyMember> newOrder)
    {
        if (newOrder.Count != _activeMembers.Count || !_activeMembers.ToHashSet().SetEquals(newOrder))
        {
            throw new ArgumentException("The new order must contain exactly the active members.", nameof(newOrder));
        }

        if (Enumerable.Range(0, newOrder.Count).Any(index => newOrder[index] != _activeMembers[index] && !CanChangePlace(_activeMembers[index])))
        {
            throw new ArgumentException("The leader must stay first.", nameof(newOrder));
        }

        _activeMembers.Clear();
        _activeMembers.AddRange(newOrder);
    }

    private void RebuildReserve()
    {
        _reserveMembers.Clear();
        _reserveMembers.AddRange(Members.Where(member => !_activeMembers.Contains(member)));
    }

    private void ThrowIfNotRecruited(PartyMember member)
    {
        if (!Members.Contains(member))
        {
            throw new ArgumentException($"'{member.Id}' is not a recruited member.", nameof(member));
        }
    }
}
