using System.ComponentModel;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The shown Database page before an undo or a redo (decision 920): its typed changes applied first, or the undo or
/// redo refused with the page's own refusal, which clears with the page's (decision 1008).
/// </summary>
internal sealed partial class CombinedHistory
{
    private readonly DatabaseWorkspaceViewModel _database;

    // The page's refusal as last copied into Refusal, so only that one clears with the page's.
    private string? _copiedPageRefusal;

    private bool TryApplyShownPage()
    {
        if (_database.TryApplyShown())
        {
            return true;
        }

        Refusal = _copiedPageRefusal = _database.Refusal;
        return false;
    }

    // Any change of the page's refusal (a fix typed, Revert, a move that passes) leaves the copy quoting what is gone.
    private void OnDatabaseChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DatabaseWorkspaceViewModel.Refusal) && Refusal is not null
            && Refusal == _copiedPageRefusal)
        {
            Refusal = null;
        }
    }
}
