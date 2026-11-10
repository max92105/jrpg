using Jrpg.Core.Content;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Renaming the shown map's id (decision 530, ADR 0042): typed in the Rename map dialog, refused while anything is
/// unsaved, written at once with every reference through Data, as the sheet purge is (decision 442); the editor then
/// carries on from the written content with the map shown under its new id, Back to previous map still working and
/// every undo history starting afresh.
/// </summary>
internal sealed partial class MainViewModel
{
    private const string SaveAllFirst = "Save all first (Ctrl+Shift+S): a rename writes every map that leads to this one.";

    // The map ShowRewritten shows after a rename: its new id, which the rebuilt open maps don't hold yet.
    private string? _shownAfterRewrite;

    /// <summary>Whether Map > Rename map may open: a map is shown and nothing is unsaved.</summary>
    public bool CanRenameMap => CurrentMap is not null && !HasUnsavedChanges;

    /// <summary>The Rename map entries' tooltip: what a rename does, or why the entries are greyed.</summary>
    public string RenameMapTip => HasUnsavedChanges ? SaveAllFirst : "Change the shown map's id; every warp into it and the new game's start follow.";

    /// <summary>The Rename map dialog's state for the shown map; null while the rename can't start.</summary>
    public MapRenamingViewModel? StartRenamingMap() =>
        CanRenameMap ? new(CurrentMap!.Map, [.. Catalog!.Maps.Keys.Union(_openMaps.All.Select(open => open.Map.Id))]) : null;

    /// <returns>
    /// False when nothing was written: the id has a problem, something is unsaved (the modal dialog rules it out; this
    /// is the backstop), or the write was refused, <see cref="SaveError"/> saying why.
    /// </returns>
    public bool TryRenameMap(MapRenamingViewModel renaming)
    {
        if (renaming.Problem is not null)
        {
            return false;
        }

        if (HasUnsavedChanges)
        {
            SaveError = SaveAllFirst;
            return false;
        }

        var shown = CurrentMap!;
        var newId = renaming.NewId.Trim();
        SaveError = null;
        ContentCatalog? written = null;
        if (!TryWrite(() => written = _writer.RenameMap(Content!.Current, shown.Map.Id, newId)))
        {
            return false;
        }

        Navigation.RenameMap(shown.Map.Id, newId);
        _shownAfterRewrite = newId;
        Content!.ReplaceAfterRewrite(written!);
        ShowMapList(Content.Current);
        return true;
    }

    // HasUnsavedChanges and what hangs on it: the Rename map and Rename folder entries' greying and their tooltips.
    private void RaiseUnsavedChanged()
    {
        OnPropertyChanged(nameof(HasUnsavedChanges));
        OnPropertyChanged(nameof(CanRenameMap));
        OnPropertyChanged(nameof(RenameMapTip));
        OnPropertyChanged(nameof(CanRenameFolder));
        OnPropertyChanged(nameof(RenameFolderTip));
    }
}
