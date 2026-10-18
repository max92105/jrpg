using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Art;

/// <summary>
/// Checks that the art content names exists (ADR 0009). It runs at startup, apart from the content loader, so content
/// tests don't need art.
/// </summary>
public static class ArtReferenceCheck
{
    /// <summary>Every tilesheet and the field sprite exist as PNGs, and every tile's cell lies inside its sheet.</summary>
    /// <returns>One line per problem, naming the file; empty when the art is complete.</returns>
    public static IReadOnlyList<string> FindProblems(ContentCatalog catalog, string artRoot)
    {
        var problems = new List<string>();
        var sheetSizes = new Dictionary<string, (int Width, int Height)>();
        foreach (var sheet in catalog.Tilesheets.Values.OrderBy(sheet => sheet.Id, StringComparer.Ordinal))
        {
            if (TryReadSize(artRoot, ArtFolders.Tilesheets, sheet.File, problems) is { } size)
            {
                sheetSizes.Add(sheet.Id, size);
            }
        }

        foreach (var tile in catalog.Tiles.Values.OrderBy(tile => tile.Id))
        {
            if (sheetSizes.TryGetValue(tile.SheetId, out var size) && !IsCellInside(tile, catalog.Tilesheets[tile.SheetId], size))
            {
                problems.Add($"{ShownPath(ArtFolders.Tilesheets, catalog.Tilesheets[tile.SheetId].File)}: tile {tile.Id}'s cell ({tile.Column}, {tile.Row}) lies outside the {size.Width}x{size.Height} image.");
            }
        }

        TryReadSize(artRoot, ArtFolders.FieldSprites, catalog.NewGame.FieldSpriteFile, problems);
        return problems;
    }

    private static bool IsCellInside(TileDefinition tile, TilesheetDefinition sheet, (int Width, int Height) size) =>
        sheet.CellLeft(tile.Column) + sheet.TileWidth <= size.Width && sheet.CellTop(tile.Row) + sheet.TileHeight <= size.Height;

    private static (int Width, int Height)? TryReadSize(string artRoot, string folder, string file, List<string> problems)
    {
        var path = Path.Combine(artRoot, folder, file);
        if (!File.Exists(path))
        {
            problems.Add($"{ShownPath(folder, file)}: not found in {artRoot}.");
            return null;
        }

        try
        {
            return PngSize.Read(path);
        }
        catch (ContentValidationException)
        {
            problems.Add($"{ShownPath(folder, file)}: not a PNG image.");
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            problems.Add($"{ShownPath(folder, file)}: {exception.Message}");
            return null;
        }
    }

    private static string ShownPath(string folder, string file) => folder.Length == 0 ? file : $"{folder}/{file}";
}
