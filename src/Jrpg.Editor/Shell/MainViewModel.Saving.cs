using System.IO;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>Save, Save all and the close question (decisions 499-501).</summary>
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
    /// Saves the tiles and objects when changed, then the shown map, each if the game would load it; otherwise keeps it
    /// unsaved and shows why. With no map shown it still saves the tiles and objects (decision 499).
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        if (Content is not { } content)
        {
            return;
        }

        SaveError = null;
        if (TryWriteCatalogs(content) && CurrentMap is { } shown)
        {
            TryWriteMap(shown, content);
        }

        ShowMapList(content.Current);
        Art!.Refresh();
    }

    /// <summary>
    /// Saves the tiles and objects when changed, then every open map with changes in id order; the first the game would
    /// refuse stops it with the reason shown, the maps written before it staying saved (decision 500).
    /// </summary>
    [RelayCommand]
    private void SaveAll()
    {
        if (Content is not { } content)
        {
            return;
        }

        SaveError = null;
        if (TryWriteCatalogs(content))
        {
            foreach (var open in _openMaps.Dirty)
            {
                if (!TryWriteMap(open, content))
                {
                    break;
                }
            }
        }

        ShowMapList(content.Current);
        Art!.Refresh();
    }

    // The catalogs first, so a saved map never names a tile the game doesn't know.
    private bool TryWriteCatalogs(CatalogDraft content) =>
        !content.IsDirty || TryWrite(() =>
        {
            _writer.WriteMapCatalogs(content.Current);
            content.MarkSaved();
        });

    private bool TryWriteMap(MapViewModel open, CatalogDraft content)
    {
        var map = open.Map;
        if (!TryWrite(() => _writer.WriteMap(map, content.Current)))
        {
            return false;
        }

        content.AddSavedMap(map);
        open.MarkSaved();
        return true;
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
