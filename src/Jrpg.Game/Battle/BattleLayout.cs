using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// Where each battler stands: enemies on the left, the party on the right, each row higher up and shifted sideways for
/// depth. A spot is the square a battler stands in, feet on its bottom edge.
/// </summary>
internal static class BattleLayout
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56).
    public const int SpotSize = 96;
    private const int RowSpacing = SpotSize + 40;
    private const int RowShift = 50;
    private const int EnemyStartX = 300 + SpotSize;
    private const int PartyStartX = VirtualScreen.Width - (400 + SpotSize);
    private const int BottomRowBottom = VirtualScreen.Height - 120;

    /// <param name="slot">The enemy's position in its encounter, from 0: each slot is a row up and a shift right.</param>
    public static Rectangle EnemySpot(int slot) => SpotOnRow(EnemyStartX + (slot * RowShift), slot);

    /// <summary>The leader stands on the top row, furthest left; the last member on the bottom row.</summary>
    /// <param name="index">The member's place in the active party, from 0.</param>
    /// <param name="count">How many members are active.</param>
    public static Rectangle PartySpot(int index, int count)
    {
        var row = count - 1 - index;
        return SpotOnRow(PartyStartX - (row * RowShift), row);
    }

    /// <param name="combatants">The party in active order, then the enemies in encounter order, as a battle lists them.</param>
    /// <returns>Each combatant's spot, in the same order.</returns>
    public static Rectangle[] SpotsOf(IReadOnlyList<Combatant> combatants)
    {
        var partyCount = combatants.Count(combatant => combatant.Side == BattleSide.Party);
        return [.. combatants.Select((combatant, index) =>
            combatant.Side == BattleSide.Party ? PartySpot(index, partyCount) : EnemySpot(index - partyCount))];
    }

    private static Rectangle SpotOnRow(int left, int row) =>
        new(left, BottomRowBottom - (row * RowSpacing) - SpotSize, SpotSize, SpotSize);
}
