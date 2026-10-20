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

    /// <returns>False for the leader or a member who isn't active.</returns>
    public bool TryMoveToReserve(PartyMember member)
    {
        ThrowIfNotRecruited(member);
        if (member == Leader || !_activeMembers.Remove(member))
        {
            return false;
        }

        RebuildReserve();
        return true;
    }

    /// <exception cref="ArgumentException">The new order has different members, or the leader isn't first.</exception>
    public void Reorder(IReadOnlyList<PartyMember> newOrder)
    {
        if (newOrder.Count != _activeMembers.Count || !_activeMembers.ToHashSet().SetEquals(newOrder))
        {
            throw new ArgumentException("The new order must contain exactly the active members.", nameof(newOrder));
        }

        if (newOrder[0] != Leader)
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
