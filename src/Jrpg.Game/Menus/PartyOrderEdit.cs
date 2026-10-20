using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>
/// The new active order when the player swaps two members' places (question 61, placeholder: pick one, then the place
/// to swap with). Core's <see cref="PartyState.Reorder"/> then checks it keeps the leader first.
/// </summary>
internal static class PartyOrderEdit
{
    /// <returns>A new list; <paramref name="order"/> is left as it was.</returns>
    public static IReadOnlyList<PartyMember> Swap(IReadOnlyList<PartyMember> order, PartyMember first, PartyMember second)
    {
        var swapped = order.ToList();
        var (firstPlace, secondPlace) = (swapped.IndexOf(first), swapped.IndexOf(second));
        (swapped[firstPlace], swapped[secondPlace]) = (second, first);
        return swapped;
    }
}
