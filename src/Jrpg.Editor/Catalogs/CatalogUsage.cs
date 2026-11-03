using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// What uses a catalog record, so an undo that would remove it can be refused (decision 117) and a re-cut can say what
/// it touches (decision 116): maps, saved or open, and objects; a map's events for a quest or a stage (decision 394).
/// The open map is read as it is on screen, unsaved changes included, in place of its saved version.
/// </summary>
internal static class CatalogUsage
{
    /// <param name="tileId">The tile looked for.</param>
    /// <param name="catalog">The saved maps and the objects.</param>
    /// <param name="openLayout">The open map's layout, unsaved changes included; null when no map is open.</param>
    /// <returns>"map forest" and "object chest" style names, maps first, each sorted by id; empty when unused.</returns>
    public static IReadOnlyList<string> UsersOfTile(int tileId, ContentCatalog catalog, MapLayout? openLayout)
    {
        var tileIds = new HashSet<int> { tileId };
        var maps = MapIdsUsing(tileIds, catalog, openLayout).Select(id => $"map {id}");
        var objects = ObjectIdsUsing(tileIds, catalog).Select(id => $"object {id}");
        return [.. maps, .. objects];
    }

    /// <summary>The tiles cut from a sheet, used or not: a tile can't outlive its sheet, so the sheet's removal waits for theirs.</summary>
    /// <returns>Their ids, lowest first.</returns>
    public static IReadOnlyList<int> TilesCutFrom(string sheetId, ContentCatalog catalog) =>
        [.. TilesOf(sheetId, catalog).Select(tile => tile.Id)];

    /// <summary>The tiles cut from a sheet and the maps and objects drawing any of them.</summary>
    /// <param name="catalog">The tiles, the saved maps and the objects.</param>
    /// <param name="sheetId">The sheet looked for; one not in the catalog has no tiles.</param>
    /// <param name="openLayout">The open map's layout, unsaved changes included; null when no map is open.</param>
    public static SheetUsage OfSheet(ContentCatalog catalog, string sheetId, MapLayout? openLayout)
    {
        IReadOnlyList<TileDefinition> tiles = [.. TilesOf(sheetId, catalog)];
        var tileIds = tiles.Select(tile => tile.Id).ToHashSet();
        return new SheetUsage(tiles, MapIdsUsing(tileIds, catalog, openLayout), ObjectIdsUsing(tileIds, catalog));
    }

    private static IEnumerable<TileDefinition> TilesOf(string sheetId, ContentCatalog catalog) =>
        catalog.Tiles.Values.Where(tile => tile.SheetId == sheetId).OrderBy(tile => tile.Id);

    /// <summary>
    /// The maps placing an object, or whose events swap to it (a swap to an unknown object fails the save, so the
    /// object can't leave while one names it), so an undo or a removal that would take it away can be refused.
    /// </summary>
    /// <param name="catalog">The saved maps.</param>
    /// <param name="objectId">The object looked for.</param>
    /// <param name="openLayout">The open map's layout, unsaved changes included; null when no map is open.</param>
    /// <returns>"map forest" style names, sorted by id; empty when no map uses it.</returns>
    public static IReadOnlyList<string> UsersOfObject(ContentCatalog catalog, string objectId, MapLayout? openLayout) =>
        [.. LayoutsOf(catalog, openLayout)
            .Where(layout => layout.Objects.Any(placed => placed.ObjectId == objectId) || SwapsTo(layout, objectId))
            .Select(layout => layout.Id)
            .Order(StringComparer.Ordinal)
            .Select(id => $"map {id}")];

    private static bool SwapsTo(MapLayout layout, string objectId) =>
        layout.AllEvents.Any(mapEvent => mapEvent.Steps.Any(step => step is SwapObjectStep swap && swap.ObjectId == objectId));

    /// <summary>
    /// The maps whose events name a quest, in a step or a condition, or name one of its stages when
    /// <paramref name="stageId"/> is given, so an undo that would remove it can be refused (decision 394).
    /// </summary>
    /// <param name="catalog">The saved maps.</param>
    /// <param name="questId">The quest looked for.</param>
    /// <param name="stageId">The stage looked for; null for the quest under any stage.</param>
    /// <param name="openLayout">The open map's layout, unsaved changes included; null when no map is open.</param>
    /// <returns>"map forest" style names, sorted by id; empty when no event names it.</returns>
    public static IReadOnlyList<string> UsersOfQuest(ContentCatalog catalog, string questId, string? stageId, MapLayout? openLayout) =>
        [.. LayoutsOf(catalog, openLayout)
            .Where(layout => layout.AllEvents.Any(mapEvent => NamesQuest(mapEvent, questId, stageId)))
            .Select(layout => layout.Id)
            .Order(StringComparer.Ordinal)
            .Select(id => $"map {id}")];

    // Every quest step names a quest and an advance names a stage too; a condition names a quest and may name a stage.
    private static bool NamesQuest(MapEvent mapEvent, string questId, string? stageId) =>
        mapEvent.Steps.Any(step => NamesQuest(step, questId, stageId))
        || (mapEvent.OnlyWhenQuest is { } gate && gate.QuestId == questId && (stageId is null || gate.StageId == stageId));

    private static bool NamesQuest(EventStep step, string questId, string? stageId) => step switch
    {
        StartQuestStep start => stageId is null && start.QuestId == questId,
        AdvanceQuestStep advance => advance.QuestId == questId && (stageId is null || advance.StageId == stageId),
        CompleteQuestStep complete => stageId is null && complete.QuestId == questId,
        _ => false,
    };

    private static IReadOnlyList<string> MapIdsUsing(HashSet<int> tileIds, ContentCatalog catalog, MapLayout? openLayout) =>
        [.. LayoutsOf(catalog, openLayout)
            .Where(layout => layout.Layers.Any(layer => layer.Rows.Any(row => row.Any(tileIds.Contains))))
            .Select(layout => layout.Id)
            .Order(StringComparer.Ordinal)];

    // Every map's layout, the open map's as it is on screen in place of its saved version.
    private static IEnumerable<MapLayout> LayoutsOf(ContentCatalog catalog, MapLayout? openLayout)
    {
        var layouts = catalog.Maps.Values.Select(map => map.Layout).Where(layout => layout.Id != openLayout?.Id);
        return openLayout is null ? layouts : layouts.Append(openLayout);
    }

    private static IReadOnlyList<string> ObjectIdsUsing(HashSet<int> tileIds, ContentCatalog catalog) =>
        [.. catalog.Objects.Values
            .Where(definition => definition.Tiles.Any(tile => tileIds.Contains(tile.TileId)))
            .Select(definition => definition.Id)
            .Order(StringComparer.Ordinal)];
}
