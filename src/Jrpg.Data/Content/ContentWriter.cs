using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Content;

/// <summary>
/// Saves content so the game will load it (ADR 0011, 0012): the loader's validators run first, and on any problem
/// nothing is written.
/// </summary>
/// <param name="contentRoot">The content folder written to, found by <see cref="ContentRootLocator"/>.</param>
public sealed class ContentWriter(string contentRoot)
{
    private const string TemporarySuffix = ".saving";

    /// <summary>
    /// Writes a map's layout and settings, replacing the map with the same id or adding a new one. Every map is checked
    /// with this one in place, since another map's warp may lead into it.
    /// </summary>
    /// <param name="map">The map to save.</param>
    /// <param name="catalog">The content the map refers to, as loaded; its version of this map, if any, is ignored.</param>
    /// <exception cref="ContentValidationException">The game would reject the map; the message is the loader's, and nothing was written.</exception>
    public void WriteMap(MapDefinition map, ContentCatalog catalog)
    {
        var layouts = catalog.Maps.Values.Select(other => other.Layout).Where(layout => layout.Id != map.Id).Append(map.Layout);
        var references = new MapReferences(ContentReferencesOf(catalog), layouts.ToDictionary(layout => layout.Id));
        foreach (var layout in references.Layouts.Values)
        {
            MapLayoutValidator.Validate(MapFolderReader.LayoutFile(layout.Id), layout, references);
        }

        var settingsFile = MapFolderReader.SettingsFile(map.Id);
        MapSettingsValidator.Validate(settingsFile, map.Settings, references.Content.EncounterIds);

        // The new game starts on a map, so changing that map can move the start position off it.
        var maps = catalog.Maps.Values.Where(other => other.Id != map.Id).Append(map).ToDictionary(other => other.Id);
        var newGameReferences = new NewGameReferences(catalog.PartyMembers.Keys.ToHashSet(), catalog.Items, catalog.Rules.Party, maps);
        NewGameValidator.Validate(ContentCatalogLoader.NewGameFile, catalog.NewGame, newGameReferences);

        WriteAll([PlanWrite(MapFolderReader.LayoutFile(map.Id), map.Layout), PlanWrite(settingsFile, map.Settings)]);
    }

    /// <summary>
    /// Writes the tilesheet, tile and object catalogs, each sorted by id, after checking them as the loader and the art
    /// check would, with every map's layout checked against them.
    /// </summary>
    /// <param name="catalog">The content holding the catalogs to save; its maps are the ones checked.</param>
    /// <exception cref="ContentValidationException">The game would reject the catalogs; the message names the file, and nothing was written.</exception>
    public void WriteMapCatalogs(ContentCatalog catalog)
    {
        var sheets = catalog.Tilesheets.Values.OrderBy(sheet => sheet.Id, StringComparer.Ordinal).ToList();
        var tiles = catalog.Tiles.Values.OrderBy(tile => tile.Id).ToList();
        var objects = catalog.Objects.Values.OrderBy(definition => definition.Id, StringComparer.Ordinal).ToList();
        TilesheetCatalogValidator.Validate(ContentCatalogLoader.TilesheetsFile, sheets);
        TileCatalogValidator.Validate(ContentCatalogLoader.TilesFile, tiles, catalog.Tilesheets.Keys.ToHashSet());
        ObjectCatalogValidator.Validate(ContentCatalogLoader.ObjectsFile, objects, catalog.Tiles.Keys.ToHashSet());
        var references = new MapReferences(ContentReferencesOf(catalog), catalog.Maps.Values.ToDictionary(map => map.Id, map => map.Layout));
        foreach (var layout in references.Layouts.Values)
        {
            MapLayoutValidator.Validate(MapFolderReader.LayoutFile(layout.Id), layout, references);
        }

        var artProblems = ArtReferenceCheck.FindProblems(catalog, ArtFolders.RootIn(contentRoot));
        if (artProblems.Count > 0)
        {
            throw new ContentValidationException(string.Join(Environment.NewLine, artProblems));
        }

        WriteAll(
        [
            PlanWrite(ContentCatalogLoader.TilesheetsFile, sheets),
            PlanWrite(ContentCatalogLoader.TilesFile, tiles),
            PlanWrite(ContentCatalogLoader.ObjectsFile, objects),
        ]);
    }

    /// <param name="fileName">A file name in the tilesheet art folder.</param>
    public bool HasTilesheetImage(string fileName) => File.Exists(TilesheetImagePath(fileName));

    /// <summary>
    /// Copies a PNG into the tilesheet art folder under its own file name, replacing one already there (the caller asks
    /// first, decision 125). A replacement must still hold every tile cut from it by the catalog's sheets.
    /// </summary>
    /// <param name="sourcePath">The image to copy, anywhere on disk.</param>
    /// <param name="catalog">The sheets and tiles the image is checked against; the editor's draft, unsaved tiles included.</param>
    /// <returns>The file name it was copied under.</returns>
    /// <exception cref="ContentValidationException">Not a PNG, or too small for a tile cut from it; nothing was copied.</exception>
    public string ImportTilesheetImage(string sourcePath, ContentCatalog catalog)
    {
        var fileName = Path.GetFileName(sourcePath);
        var size = PngSize.Read(sourcePath);
        if (ArtReferenceCheck.FirstTileOutside(catalog, fileName, size) is { } outside)
        {
            throw new ContentValidationException(
                $"{fileName}: tile {outside.Id} at ({outside.Column}, {outside.Row}) would be outside the new {size.Width} x {size.Height} image.");
        }

        var destination = TilesheetImagePath(fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(sourcePath, destination + TemporarySuffix, overwrite: true);
        File.Move(destination + TemporarySuffix, destination, overwrite: true);
        return fileName;
    }

    private string TilesheetImagePath(string fileName) => Path.Combine(ArtFolders.RootIn(contentRoot), ArtFolders.Tilesheets, fileName);

    private static MapContentReferences ContentReferencesOf(ContentCatalog catalog) =>
        new(
            catalog.Tiles.Keys.ToHashSet(),
            catalog.Objects,
            catalog.Items.Keys.ToHashSet(),
            catalog.Encounters.Keys.ToHashSet());

    private FileToWrite PlanWrite<T>(string file, T content) =>
        new(Path.Combine(contentRoot, file), path => ContentFileWriter.Write(path, content));

    // Every file is written aside first, so a failed write leaves the saved files whole and the folder as it was.
    private static void WriteAll(IReadOnlyList<FileToWrite> files)
    {
        try
        {
            foreach (var (path, writeTo) in files)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                writeTo(path + TemporarySuffix);
            }
        }
        catch
        {
            foreach (var (path, _) in files)
            {
                File.Delete(path + TemporarySuffix);
            }

            throw;
        }

        foreach (var (path, _) in files)
        {
            File.Move(path + TemporarySuffix, path, overwrite: true);
        }
    }

    // A content file's full path, and how to write its content to a given path: the file aside first, then the real one.
    private sealed record FileToWrite(string Path, Action<string> WriteTo);
}
