using System.IO;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>Save, Save all and the close question (decisions 499-501, 518).</summary>
internal sealed partial class MainViewModel
{
    /// <summary>
    /// Whether the editor may close: nothing unsaved, or the developer's answer to the close question: Save all closes
    /// only when everything was written, Don't save closes, Cancel stays.
    /// </summary>
    public bool ConfirmClose()
    {
        // A stroke still being dragged counts as a change, so it ends before the question.
        CurrentMap?.EndStroke();
        if (UnsavedChangesQuestion() is not { } question)
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
    /// keeps them unsaved and shows why (decisions 499, 518). With no map shown it still saves the tiles and objects.
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        if (Content is not { } content)
        {
            return;
        }

        SaveError = null;
        if (CurrentMap is { } shown)
        {
            SaveTogether(content, OpenMaps.ContentWith(content.Current, shown), [shown]);
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
        if (Content is not { } content)
        {
            return;
        }

        SaveError = null;
        SaveTogether(content, _openMaps.ContentWith(content.Current), _openMaps.Dirty);
        ShowMapList(content.Current);
        Art!.Refresh();
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

    // "Unsaved changes in map 'forest', map 'start' and the tiles and objects."; null when nothing is unsaved.
    private string? UnsavedChangesQuestion()
    {
        List<string> unsaved = [.. _openMaps.Dirty.Select(open => $"map '{open.Map.Id}'")];
        if (Content is { IsDirty: true })
        {
            unsaved.Add("the tiles and objects");
        }

        return unsaved switch
        {
            [] => null,
            [var only] => $"Unsaved changes in {only}.",
            [.. var first, var last] => $"Unsaved changes in {string.Join(", ", first)} and {last}.",
        };
    }
}
