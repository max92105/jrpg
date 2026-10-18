using Jrpg.Core.Content;
using Jrpg.Core.Maps;
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

        Write(Path.Combine(contentRoot, MapFolderReader.LayoutFile(map.Id)), map.Layout, Path.Combine(contentRoot, settingsFile), map.Settings);
    }

    private static MapContentReferences ContentReferencesOf(ContentCatalog catalog) =>
        new(
            catalog.Tiles.Keys.ToHashSet(),
            catalog.Objects,
            catalog.Items.Keys.ToHashSet(),
            catalog.Encounters.Keys.ToHashSet());

    // Both files are written aside first, so a failed write leaves the saved map whole and the folder as it was.
    private static void Write(string layoutPath, MapLayout layout, string settingsPath, MapSettings settings)
    {
        try
        {
            WriteAside(layoutPath, layout);
            WriteAside(settingsPath, settings);
        }
        catch
        {
            File.Delete(layoutPath + TemporarySuffix);
            File.Delete(settingsPath + TemporarySuffix);
            throw;
        }

        File.Move(layoutPath + TemporarySuffix, layoutPath, overwrite: true);
        File.Move(settingsPath + TemporarySuffix, settingsPath, overwrite: true);
    }

    private static void WriteAside<T>(string path, T content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        ContentFileWriter.Write(path + TemporarySuffix, content);
    }
}
