using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Save, Save all and the close question (decisions 499-501, 518); a refused Save points to Save all when the open maps
/// pass together (decision 519). Each first applies the shown Database page: one that can't apply stops a save with
/// nothing written, its refusal on the save line until the page's clears, and is listed by the close question
/// (decisions 923, 924, 1008).
/// </summary>
internal sealed partial class MainViewModel
{
    // The shown page's refusal as last quoted in SaveError, so only that line clears with the page's.
    private string? _pageSaveError;

    /// <summary>
    /// Whether the editor may close: nothing unsaved, or the developer's answer to the close question: Save all closes
    /// only when everything was written, Don't save closes, Cancel stays.
    /// </summary>
    public bool ConfirmClose()
    {
        // A stroke still being dragged counts as a change, so it ends before the question.
        CurrentMap?.EndStroke();

        // A page whose typed changes apply counts as the catalogs' change; one that can't is listed, and Don't save
        // then closes without applying it again (decision 924).
        _ = DatabaseWorkspace?.TryApplyShown();
        if (UnsavedChanges is not { } question)
        {
            return true;
        }

        switch (_askToClose(question))
        {
            case UnsavedChangesAnswer.SaveAll:
                SaveAll();
                return SaveError is null;
            case UnsavedChangesAnswer.Discard:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Saves the shown map and the tiles and objects when changed, in one go, if the game would load them; otherwise
    /// keeps them unsaved and shows why, with the way out when it is Save all (decisions 499, 518, 519). With no map
    /// shown it still saves the tiles and objects.
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        if (Content is not { } content || !TryApplyShownPage())
        {
            return;
        }

        SaveError = null;
        if (CurrentMap is { } shown)
        {
            SaveTogether(content, OpenMaps.ContentWith(content.Current, shown), [shown]);
            if (SaveError is not null && SaveAllHint(content, shown) is { } hint)
            {
                SaveError += hint;
            }
        }
        else if (content.IsDirty && TryWrite(() => _writer.WriteMapCatalogs(content.Current)))
        {
            content.MarkSaved();
        }

        ShowMapList(content.Current);
        Art!.Refresh();
    }

    /// <summary>
    /// Saves every open map with changes and the tiles and objects when changed, in one go, all or none, each map
    /// checked with the open maps as they are now (decision 518): two maps valid only together save.
    /// </summary>
    [RelayCommand]
    private void SaveAll()
    {
        if (Content is not { } content || !TryApplyShownPage())
        {
            return;
        }

        SaveError = null;
        SaveTogether(content, _openMaps.ContentWith(content.Current), _openMaps.Dirty);
        ShowMapList(content.Current);
        Art!.Refresh();
    }

    // The shown Database page's typed changes are written with the rest, or nothing is (decision 923).
    private bool TryApplyShownPage()
    {
        if (DatabaseWorkspace is not { } workspace || workspace.TryApplyShown())
        {
            return true;
        }

        SaveError = _pageSaveError = $"{workspace.Refusal} Nothing was written.";
        return false;
    }

    // Any change of the page's refusal (a fix typed, Revert, a move that passes) leaves the save line quoting what is
    // gone; a map's or a file's refusal stays.
    private void OnDatabaseWorkspaceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DatabaseWorkspaceViewModel.Refusal) && SaveError is not null
            && SaveError == _pageSaveError)
        {
            SaveError = null;
        }
    }

    // All or none; refused, the changed tiles and objects are still written alone when they pass by themselves, as
    // Ctrl+S wrote them first before decision 518, so composer work never waits on a map.
    private void SaveTogether(CatalogDraft content, ContentCatalog checkedContent, IReadOnlyList<MapViewModel> maps)
    {
        var mapIds = maps.Select(map => map.Map.Id).ToList();
        var written = TryWrite(() =>
        {
            if (content.IsDirty)
            {
                _writer.WriteMapCatalogsAndMaps(checkedContent, mapIds);
            }
            else
            {
                _writer.WriteMaps(checkedContent, mapIds);
            }
        });
        if (!written)
        {
            WriteCatalogsAlone(content);
            return;
        }

        content.MarkSaved();
        foreach (var map in maps)
        {
            content.AddSavedMap(map.Map);
            map.MarkSaved();
        }
    }

    // The map's refusal stays shown, ending with what was written.
    private void WriteCatalogsAlone(CatalogDraft content)
    {
        var mapRefusal = SaveError;
        if (content.IsDirty && TryWrite(() => _writer.WriteMapCatalogs(content.Current)))
        {
            content.MarkSaved();
            SaveError = $"{mapRefusal} The tiles and objects were saved; no map was written.";
        }
        else
        {
            SaveError = $"{mapRefusal} Nothing was written.";
        }
    }

    private bool TryWrite(Action write)
    {
        try
        {
            write();
            return true;
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            SaveError = exception.Message;
            return false;
        }
    }

    // Ctrl+S checks the other maps as saved; when that is what refused it and every open map passes as it is now,
    // Save all is the way (decision 519). The hint names the other maps with changes, which Save all writes with the
    // shown one. A refusal from elsewhere (a file that can't be written, a catalog check) gets no hint.
    private string? SaveAllHint(CatalogDraft content, MapViewModel shown)
    {
        List<string> others = [.. _openMaps.Dirty.Where(open => open != shown).Select(open => $"map '{open.Map.Id}'")];
        if (others.Count == 0 || MapsPass(OpenMaps.ContentWith(content.Current, shown)) || !MapsPass(_openMaps.ContentWith(content.Current)))
        {
            return null;
        }

        return $" Save all (Ctrl+Shift+S) writes {Named(others)} with it, which makes this valid.";
    }

    private static bool MapsPass(ContentCatalog content)
    {
        try
        {
            ContentWriter.CheckMaps(content);
            return true;
        }
        catch (ContentValidationException)
        {
            return false;
        }
    }

    /// <summary>
    /// The line the close question and the unexpected-error box (ADR 0050) show: "Unsaved changes in map 'forest', map
    /// 'start' and the tiles and objects.", a Database page that can't apply named last (decision 924); null when
    /// nothing is unsaved.
    /// </summary>
    public string? UnsavedChanges
    {
        get
        {
            List<string> unsaved = [.. _openMaps.Dirty.Select(open => $"map '{open.Map.Id}'")];
            if (Content is { IsDirty: true })
            {
                unsaved.Add("the tiles and objects");
            }

            if (DatabaseWorkspace?.UnappliedPageLine is { } unappliedPageLine)
            {
                unsaved.Add(unappliedPageLine);
            }

            return unsaved.Count == 0 ? null : $"Unsaved changes in {Named(unsaved)}.";
        }
    }

    // "map 'forest'", "map 'forest' and map 'start'", "map 'forest', map 'start' and the tiles and objects".
    private static string Named(List<string> names) =>
        names.Count == 1 ? names[0] : $"{string.Join(", ", names[..^1])} and {names[^1]}";
}
