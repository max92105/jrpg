using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
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
    public MainViewModel(string contentRoot)
    {
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
        SelectedMapId = MapIds.Count > 0 ? MapIds[0] : null;
    }

    public string ArtRoot { get; }

    public ContentCatalog? Catalog { get; }

    /// <summary>Why the content can't be edited, naming the file; null when it loaded.</summary>
    public string? LoadError { get; }

    public IReadOnlyList<string> MapIds { get; }

    [ObservableProperty]
    public partial string? SelectedMapId { get; set; }

    [ObservableProperty]
    public partial MapViewModel? CurrentMap { get; private set; }

    /// <summary>Whether the cell grid is drawn over the map; on by default, as in the legacy editor.</summary>
    [ObservableProperty]
    public partial bool IsGridShown { get; set; } = true;

    /// <summary>What the status bar says about the cell under the cursor; empty when the cursor is off the map.</summary>
    [ObservableProperty]
    public partial string CursorStatus { get; private set; } = "";

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

    partial void OnSelectedMapIdChanged(string? value) =>
        CurrentMap = value is null ? null : new MapViewModel(Catalog!.Maps[value]);
}
