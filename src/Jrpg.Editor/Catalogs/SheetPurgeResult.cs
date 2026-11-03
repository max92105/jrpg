using Jrpg.Core.Content;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The content without a sheet and everything drawn from it (decision 442's purge), and what went, for the question
/// asked before it is written.
/// </summary>
/// <param name="Catalog">
/// The purged content: the sheet, its tiles and the objects made of them gone, their cells erased on every map.
/// </param>
/// <param name="ChangedMapIds">The maps that lost a cell or a placement, sorted; only these are written.</param>
/// <param name="Cells">How many map cells held one of the sheet's tiles, over every layer of every map.</param>
/// <param name="Objects">How many object definitions used one of its tiles.</param>
/// <param name="Placements">How many placements of those objects the maps held.</param>
/// <param name="PlacementsWithEvents">How many of those placements carried events, which go with them.</param>
internal sealed record SheetPurgeResult(
    ContentCatalog Catalog,
    IReadOnlyList<string> ChangedMapIds,
    int Cells,
    int Objects,
    int Placements,
    int PlacementsWithEvents);
