using Jrpg.Core.Maps;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// A map's settings with an encounter linked to its random battles or taken out of them, as the Encounters page's Link
/// and Remove do without the map's own dialog.
/// </summary>
internal static class MapEncounterLink
{
    /// <summary>Whether the map's random battles can pick the encounter.</summary>
    public static bool IsLinked(MapSettings settings, string encounterId) =>
        settings.RandomEncounters?.EncounterIds.Contains(encounterId) == true;

    /// <summary>
    /// The settings of a map that has random battles, the encounter added last to those they pick from.
    /// </summary>
    /// <exception cref="InvalidOperationException">The map has no random battles: its odds are asked first.</exception>
    public static MapSettings Linked(MapSettings settings, string encounterId)
    {
        var table = settings.RandomEncounters
            ?? throw new InvalidOperationException($"Map '{settings.Id}' has no random battles to link to.");
        return settings with { RandomEncounters = table with { EncounterIds = [.. table.EncounterIds, encounterId] } };
    }

    /// <summary>
    /// The settings without the encounter; random battles are turned off when it was the last, since the game refuses a
    /// map whose battles pick from nothing, and its odds are asked again at the next link.
    /// </summary>
    public static MapSettings Unlinked(MapSettings settings, string encounterId)
    {
        if (settings.RandomEncounters is not { } table)
        {
            return settings;
        }

        IReadOnlyList<string> left = [.. table.EncounterIds.Where(id => id != encounterId)];
        return settings with { RandomEncounters = left.Count == 0 ? null : table with { EncounterIds = left } };
    }
}
