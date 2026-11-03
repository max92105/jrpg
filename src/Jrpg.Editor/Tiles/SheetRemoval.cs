using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Takes the shown sheet out of the catalog with every tile cut from it, as one undo step, when no map or object uses
/// any of them (decision 443: unused only, never a cascade); says what it did or why it refused. The image stays in the
/// folder, listed "(not set up)".
/// </summary>
internal sealed partial class SheetRemoval : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly TileBrowserViewModel _browser;
    private readonly Func<MapViewModel?> _openMap;

    /// <param name="catalog">The sheets and tiles.</param>
    /// <param name="browser">Whose shown sheet is removed.</param>
    /// <param name="openMap">
    /// The open map, read as it is on screen in place of its saved file; null when none is open.
    /// </param>
    public SheetRemoval(CatalogDraft catalog, TileBrowserViewModel browser, Func<MapViewModel?> openMap)
    {
        _catalog = catalog;
        _browser = browser;
        _openMap = openMap;
        browser.PropertyChanged += OnBrowserChanged;
    }

    /// <summary>
    /// What the last Remove sheet did, or why it refused, naming the users; cleared when the shown sheet changes.
    /// </summary>
    [ObservableProperty]
    public partial string? Status { get; private set; }

    [RelayCommand(CanExecute = nameof(IsSheetShown))]
    private void RemoveSheet()
    {
        var sheetId = _browser.SelectedSheet!.Id;
        var usage = CatalogUsage.OfSheet(_catalog.Current, sheetId, _openMap()?.Map.Layout);
        if (usage.MapIds.Count > 0 || usage.ObjectIds.Count > 0)
        {
            Status = $"Can't remove sheet {sheetId}: its tiles are used on {UsersText(usage)}.";
            return;
        }

        var tileCount = usage.Tiles.Count;
        _catalog.RemoveTilesheet(sheetId);
        Status = $"Removed sheet {sheetId} and its {tileCount} {(tileCount == 1 ? "tile" : "tiles")}.";
    }

    private bool IsSheetShown() => _browser.SelectedSheet is not null;

    // "maps forest, start and object tree".
    private static string UsersText(SheetUsage usage)
    {
        var maps = usage.MapIds.Count == 0 ? null : $"{(usage.MapIds.Count == 1 ? "map" : "maps")} {string.Join(", ", usage.MapIds)}";
        var objects = usage.ObjectIds.Count == 0 ? null : $"{(usage.ObjectIds.Count == 1 ? "object" : "objects")} {string.Join(", ", usage.ObjectIds)}";
        return string.Join(" and ", new[] { maps, objects }.OfType<string>());
    }

    private void OnBrowserChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TileBrowserViewModel.SelectedChoice))
        {
            Status = null;
            RemoveSheetCommand.NotifyCanExecuteChanged();
        }
    }
}
