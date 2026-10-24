using Jrpg.Core.Content;
using Jrpg.Core.Events;

namespace Jrpg.Data.Maps;

/// <summary>The steps of the maps' events, for the checks that a file a step names exists.</summary>
internal static class MapEventSteps
{
    /// <summary>
    /// Every step of one kind in the maps' object and cell events, with the event and map that hold it, in a stable
    /// order for the report.
    /// </summary>
    public static IEnumerable<(TStep Step, string NamedBy)> Of<TStep>(ContentCatalog catalog)
        where TStep : EventStep =>
        from map in catalog.Maps.Values.OrderBy(map => map.Id, StringComparer.Ordinal)
        from mapEvent in map.Layout.Objects.SelectMany(placed => placed.Events).Concat(map.Layout.CellEvents.Select(cell => cell.Event))
        from step in mapEvent.Steps.OfType<TStep>()
        select (step, $"event '{mapEvent.Id}' on map '{map.Id}'");
}
