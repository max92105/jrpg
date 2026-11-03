using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Takes the shown sheet out of the catalog. With its image there: with every tile cut from it, as one undo step, when
/// no map or object uses any of them (decision 443: unused only). With its image missing: the purge (decision 442), its
/// tiles, their cells on every map and the objects made of them, asked first through <see cref="PurgeQuestion"/>, then
/// written at once and not undoable. Says what it did or why it refused.
/// </summary>
internal sealed partial class SheetRemoval : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly TileBrowserViewModel _browser;
    private readonly Func<MapViewModel?> _openMap;
    private readonly ContentWriter _writer;

    /// <param name="catalog">The sheets and tiles.</param>
    /// <param name="browser">Whose shown sheet is removed.</param>
    /// <param name="openMap">
    /// The open map, read as it is on screen in place of its saved file; null when none is open.
    /// </param>
    /// <param name="writer">Writes the purge, which goes to disk at once.</param>
    public SheetRemoval(CatalogDraft catalog, TileBrowserViewModel browser, Func<MapViewModel?> openMap, ContentWriter writer)
    {
        _catalog = catalog;
        _browser = browser;
        _openMap = openMap;
        _writer = writer;
        browser.PropertyChanged += OnBrowserChanged;
    }

    /// <summary>
    /// What the last Remove sheet did, or why it refused, naming the users; cleared when the shown sheet changes.
    /// </summary>
    [ObservableProperty]
    public partial string? Status { get; private set; }

    /// <summary>
    /// The purge waiting for the developer's yes or no; the window asks it, then applies or drops it.
    /// </summary>
    [ObservableProperty]
    public partial SheetPurgeQuestion? PurgeQuestion { get; private set; }

    /// <summary>
    /// Writes the purge asked, then the editor carries on from the written content; on a refused write, says why.
    /// </summary>
    public void ApplyPurge()
    {
        var question = PurgeQuestion!;
        PurgeQuestion = null;
        try
        {
            _writer.WriteMapCatalogsAndMaps(question.Purge.Catalog, question.Purge.ChangedMapIds);
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            Status = $"Not removed: {exception.Message}";
            return;
        }

        _catalog.ReplaceAfterRewrite(question.Purge.Catalog);
        Status = $"Removed sheet {question.SheetId} and everything drawn from it; saved.";
    }

    public void DropPurge() => PurgeQuestion = null;

    [RelayCommand(CanExecute = nameof(IsSheetShown))]
    private void RemoveSheet()
    {
        if (_browser.SelectedChoice!.IsImageMissing)
        {
            AskPurge();
            return;
        }

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

    // The purge writes straight away, so it starts from saved content; a staying event naming a removed object stops
    // it.
    private void AskPurge()
    {
        var sheetId = _browser.SelectedSheet!.Id;
        if (_catalog.IsDirty || _openMap() is { IsDirty: true })
        {
            Status = $"Save first: removing sheet {sheetId} writes the content at once.";
            return;
        }

        if (SheetPurge.RefusalOf(_catalog.Current, sheetId) is { } refusal)
        {
            Status = refusal;
            return;
        }

        PurgeQuestion = SheetPurgeQuestion.For(sheetId, _catalog.Current, SheetPurge.Purge(_catalog.Current, sheetId));
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
