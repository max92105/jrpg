namespace Jrpg.Editor.Shell;

/// <summary>
/// The order of the steps recorded in the open map's history and in the catalogs' (ADR 0014), so one Undo and one Redo
/// go to the newest step, whichever history holds it. Only the open map's steps are kept: its history goes with it.
/// </summary>
internal sealed class UndoJournal
{
    private readonly List<UndoTarget> _done = [];
    private readonly List<UndoTarget> _undone = [];

    public bool CanUndo => _done.Count > 0;

    public bool CanRedo => _undone.Count > 0;

    /// <summary>A step was recorded in one history; undone steps can no longer be redone, as in each history.</summary>
    public void Record(UndoTarget step)
    {
        _done.Add(step);
        _undone.Clear();
    }

    /// <returns>The history whose newest step to undo.</returns>
    public UndoTarget Undo() => Move(_done, _undone);

    /// <returns>The history whose newest undone step to redo.</returns>
    public UndoTarget Redo() => Move(_undone, _done);

    /// <summary>Another map opened: the closed map's history, and so its steps here, are gone.</summary>
    public void ForgetMapSteps()
    {
        _done.RemoveAll(step => step == UndoTarget.Map);
        _undone.RemoveAll(step => step == UndoTarget.Map);
    }

    /// <summary>The catalogs dropped their undone steps outside any recorded step; the map's stay redoable.</summary>
    public void ForgetUndoneCatalogSteps() => _undone.RemoveAll(step => step == UndoTarget.Catalogs);

    private static UndoTarget Move(List<UndoTarget> from, List<UndoTarget> to)
    {
        var step = from[^1];
        from.RemoveAt(from.Count - 1);
        to.Add(step);
        return step;
    }
}
