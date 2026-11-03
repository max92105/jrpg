using Jrpg.Core.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// What Remove sheet asks before the purge of a sheet whose image is missing (decision 442), and the purge it would
/// write.
/// </summary>
/// <param name="SheetId">The sheet that goes.</param>
/// <param name="Text">The yes or no question, with what goes counted.</param>
/// <param name="Purge">The content to write on yes.</param>
internal sealed record SheetPurgeQuestion(string SheetId, string Text, SheetPurgeResult Purge)
{
    /// <param name="sheetId">The sheet that goes.</param>
    /// <param name="before">The content before the purge, for the ids of the objects that go.</param>
    /// <param name="purge">What goes.</param>
    public static SheetPurgeQuestion For(string sheetId, ContentCatalog before, SheetPurgeResult purge)
    {
        var tiles = before.Tiles.Count - purge.Catalog.Tiles.Count;
        var parts = new List<string> { $"takes out its {Counted(tiles, "tile")}" };
        if (purge.Cells > 0)
        {
            parts.Add($"erases {Counted(purge.Cells, "cell")} on {(purge.ChangedMapIds.Count == 1 ? "map" : "maps")} {string.Join(", ", purge.ChangedMapIds)}");
        }

        if (purge.Objects > 0)
        {
            var objectIds = before.Objects.Keys.Except(purge.Catalog.Objects.Keys).Order(StringComparer.Ordinal);
            parts.Add($"removes {(purge.Objects == 1 ? "object" : "objects")} {string.Join(", ", objectIds)} with their {Counted(purge.Placements, "placement")} ({purge.PlacementsWithEvents} with events)");
        }

        var text = $"Sheet {sheetId}'s image is missing. Removing it {string.Join(", ", parts)}. This is written at once and can't be undone. Remove?";
        return new SheetPurgeQuestion(sheetId, text, purge);
    }

    private static string Counted(int count, string thing) => count == 1 ? $"1 {thing}" : $"{count} {thing}s";
}
