using Jrpg.Core.Battle;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>
/// Which background a battle on a map is fought on (decision 892); the battle screen reads it through
/// <see cref="ExplorationState.PendingBattleBackground"/>.
/// </summary>
public static class EncounterBackground
{
    /// <returns>
    /// The encounter's own, a boss's arena, else the map's; null when neither names one, which the loader refuses for a
    /// map with random battles.
    /// </returns>
    public static string? On(EncounterDefinition encounter, MapSettings map) =>
        encounter.BackgroundFile ?? map.BattleBackgroundFile;
}
