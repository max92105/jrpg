using Jrpg.Core.Maps;

namespace Jrpg.Editor.Encounters;

/// <summary>One map in the Encounters page's linked list or in its Link dialog's box.</summary>
/// <param name="MapId">The map's id.</param>
/// <param name="Label">"Old Forest (old_forest)": its name as the map list shows it, then its id.</param>
internal sealed record MapLinkChoice(string MapId, string Label)
{
    public static MapLinkChoice Of(MapDefinition map) => new(map.Id, $"{map.Settings.DisplayName} ({map.Id})");

    /// <summary>The maps as choices, by name then id, as the map list sorts them.</summary>
    public static IReadOnlyList<MapLinkChoice> AllOf(IEnumerable<MapDefinition> maps) =>
        [.. maps
            .OrderBy(map => map.Settings.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(map => map.Id, StringComparer.Ordinal)
            .Select(Of)];
}
