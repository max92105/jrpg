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

    /// <summary>
    /// Every background a battle on the map can be fought on, which the game decodes on entering it (decision 900).
    /// </summary>
    /// <param name="map">The map whose own background and random battles are read.</param>
    /// <param name="encounters">Every encounter by id, holding each one the map's random battles list.</param>
    /// <returns>
    /// The map's background, then each listed encounter's own, each file once, in that order; empty when the map names
    /// none and has no random battles.
    /// </returns>
    public static IReadOnlyList<string> PossibleOn(MapSettings map, IReadOnlyDictionary<string, EncounterDefinition> encounters)
    {
        var files = new List<string>();
        AddOnce(files, map.BattleBackgroundFile);
        foreach (var encounterId in map.RandomEncounters?.EncounterIds ?? [])
        {
            AddOnce(files, encounters[encounterId].BackgroundFile);
        }

        return files;
    }

    private static void AddOnce(List<string> files, string? file)
    {
        if (file is not null && !files.Contains(file))
        {
            files.Add(file);
        }
    }
}
