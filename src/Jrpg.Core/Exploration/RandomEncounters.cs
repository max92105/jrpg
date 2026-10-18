using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;

namespace Jrpg.Core.Exploration;

/// <summary>
/// The one place random battles are rolled, once per step (developer decision 26). Steps are counted from the last map
/// entry or fight; the first <see cref="RandomEncounterTable.GraceSteps"/> never roll, so battles never come back to back.
/// </summary>
internal sealed class RandomEncounters(IRandom random)
{
    private int _stepsSinceReset;

    /// <summary>Counts one step and rolls for a battle when the map has random battles and the grace steps are over.</summary>
    /// <returns>The id of the encounter that starts, or null. Draws nothing when no roll is due.</returns>
    public string? RollStep(RandomEncounterTable? table)
    {
        _stepsSinceReset++;
        if (table is null || _stepsSinceReset <= table.GraceSteps)
        {
            return null;
        }

        if (random.NextBelow(table.OneInSteps) != 0)
        {
            return null;
        }

        return table.EncounterIds[random.NextBelow(table.EncounterIds.Count)];
    }

    /// <summary>Starts the grace steps again: on entering a map and after any fight.</summary>
    public void ResetSteps() => _stepsSinceReset = 0;
}
