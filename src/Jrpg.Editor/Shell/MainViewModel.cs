using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window's state: the loaded content, the maps to choose from and the open one. Content that can't load is
/// shown with the loader's message instead of crashing, so a broken file can be found and fixed.
/// </summary>
internal sealed partial class MainViewModel : ObservableObject
{
    private readonly ContentWriter _writer;
    private readonly Func<string, bool> _confirmDiscard;

    /// <param name="contentRoot">The content folder to edit.</param>
    /// <param name="confirmDiscard">Asks the user whether to drop unsaved changes, given the question; true to drop them.</param>
    public MainViewModel(string contentRoot, Func<string, bool> confirmDiscard)
    {
        _writer = new ContentWriter(contentRoot);
        _confirmDiscard = confirmDiscard;
        ArtRoot = ArtFolders.RootIn(contentRoot);
        try
        {
            Catalog = ContentCatalogLoader.Load(contentRoot);
            var artProblems = ArtReferenceCheck.FindProblems(Catalog, ArtRoot);
            LoadError = artProblems.Count == 0 ? null : string.Join(Environment.NewLine, artProblems);
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            LoadError = exception.Message;
        }

        // Maps only open when their content and art are whole, since drawing reads both.
        MapIds = Catalog is not null && LoadError is null ? [.. Catalog.Maps.Keys.Order(StringComparer.Ordinal)] : [];
        if (MapIds.Count > 0)
        {
            TryOpenMap(MapIds[0]);
        }
    }

    public string ArtRoot { get; }

    /// <summary>The content as last loaded or saved; a save puts the saved map in it, so later saves check against it.</summary>
    public ContentCatalog? Catalog { get; private set; }

    /// <summary>Why the content can't be edited, naming the file; null when it loaded.</summary>
    public string? LoadError { get; }

    public IReadOnlyList<string> MapIds { get; }

    [ObservableProperty]
    public partial MapViewModel? CurrentMap { get; private set; }

    /// <summary>Why the last save was refused, in the loader's words; null after a successful save.</summary>
    [ObservableProperty]
    public partial string? SaveError { get; private set; }

    /// <summary>Whether the cell grid is drawn over the map; on by default, as in the legacy editor.</summary>
    [ObservableProperty]
    public partial bool IsGridShown { get; set; } = true;

    /// <summary>What the status bar says about the cell under the cursor; empty when the cursor is off the map.</summary>
    [ObservableProperty]
    public partial string CursorStatus { get; private set; } = "";

    /// <summary>Opens another map, asking first if the open one has unsaved changes.</summary>
    /// <returns>False when the user kept the open map.</returns>
    public bool TryOpenMap(string mapId)
    {
        if (CurrentMap?.Map.Id == mapId)
        {
            return true;
        }

        if (!ConfirmLeaveCurrentMap())
        {
            return false;
        }

        CurrentMap = new MapViewModel(Catalog!.Maps[mapId]);
        SaveError = null;
        return true;
    }

    /// <summary>Whether the editor may close: nothing unsaved, or the user agreed, when asked, to drop it.</summary>
    public bool ConfirmClose() => ConfirmLeaveCurrentMap();

    /// <param name="cell">The cell under the cursor, or null when the cursor left the view.</param>
    public void ShowCursorAt(GridCell? cell)
    {
        if (cell is not { } at || CurrentMap is null || !CurrentMap.Map.Layout.Contains(at))
        {
            CursorStatus = "";
            return;
        }

        var position = $"Cell ({at.X}, {at.Y})";
        CursorStatus = CurrentMap.TopTileAt(at) is { } tile ? $"{position}   Tile {tile.TileId} on layer z {tile.LayerZ}" : $"{position}   No tile";
    }

    /// <summary>Saves the open map if the game would load it; otherwise keeps it unsaved and shows why.</summary>
    [RelayCommand]
    private void Save()
    {
        if (CurrentMap is null)
        {
            return;
        }

        var map = CurrentMap.Map;
        try
        {
            _writer.WriteMap(map, Catalog!);
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            SaveError = exception.Message;
            return;
        }

        Catalog = Catalog! with { Maps = new Dictionary<string, MapDefinition>(Catalog.Maps) { [map.Id] = map } };
        CurrentMap.MarkSaved();
        SaveError = null;
    }

    private bool ConfirmLeaveCurrentMap() =>
        CurrentMap is not { IsDirty: true } open
        || _confirmDiscard($"Map '{open.Map.Id}' has unsaved changes. Discard them?");
}
