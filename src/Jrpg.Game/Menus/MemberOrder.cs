using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>The order the member pages go through the party in.</summary>
internal static class MemberOrder
{
    /// <summary>Everyone recruited: the active party in its order, then the reserve.</summary>
    public static IReadOnlyList<PartyMember> ActiveThenReserve(PartyState party)
    {
        var active = party.ActiveMembers;
        return [.. active, .. party.Members.Where(member => !active.Contains(member))];
    }
}
