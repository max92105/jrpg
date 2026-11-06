namespace Jrpg.Editor.Shell;

/// <summary>
/// The order of the steps recorded in the open maps' histories and the catalogs' (ADR 0014), so one Undo and one Redo
/// go to the newest step, whichever history holds it. Undo and Redo go to the newest step of the shown map or the
/// catalogs; the other maps' steps stay in place for when they are shown again (decision 503).
/// </summary>
internal sealed class UndoJournal
{
    private readonly List<UndoStep> _done = [];
    private readonly List<UndoStep> _undone = [];

    /// <param name="shownMapId">The shown map's id; null when no map is shown.</param>
    public bool CanUndo(string? shownMapId) => IndexOfNewest(_done, shownMapId) >= 0;

    /// <param name="shownMapId">The shown map's id; null when no map is shown.</param>
    public bool CanRedo(string? shownMapId) => IndexOfNextRedo(shownMapId) >= 0;

    /// <summary>The step the next undo goes to; null when there is nothing to undo.</summary>
    /// <param name="shownMapId">The shown map's id; null when no map is shown.</param>
    public UndoStep? NextUndo(string? shownMapId) => IndexOfNewest(_done, shownMapId) is var index and >= 0 ? _done[index] : null;

    /// <summary>
    /// A step was recorded in one history; every undone step can no longer be redone, as in each history: a new edit
    /// could start using what a redo would take away.
    /// </summary>
    public void Record(UndoStep step)
    {
        _done.Add(step);
        _undone.Clear();
    }

    /// <returns>The step whose history to undo.</returns>
    public UndoStep Undo(string? shownMapId) => Move(_done, _undone, IndexOfNewest(_done, shownMapId));

    /// <returns>The step whose history to redo.</returns>
    public UndoStep Redo(string? shownMapId) => Move(_undone, _done, IndexOfNextRedo(shownMapId));

    /// <summary>
    /// The content was rewritten outside every history (the sheet purge): no step is left to undo or redo.
    /// </summary>
    public void Clear()
    {
        _done.Clear();
        _undone.Clear();
    }

    /// <summary>The catalogs dropped their undone steps outside any recorded step; the maps' stay redoable.</summary>
    public void ForgetUndoneCatalogSteps() => _undone.RemoveAll(step => step.Target == UndoTarget.Catalogs);

    // The catalogs' steps go with every map; a map's only with that map.
    private static int IndexOfNewest(List<UndoStep> steps, string? shownMapId) =>
        steps.FindLastIndex(step => step.Target == UndoTarget.Catalogs || step.MapId == shownMapId);

    // Another map's undone step newer than a catalog step holds it back: that map, as it is now, may use what the
    // catalog redo would remove (an undone removal of a placed object, then Remove object, both undone). Undo needs no
    // such rule, since a catalog undo is refused while any open map uses what it removes. A step under a held catalog
    // step waits for it, so nothing is redone out of order.
    private int IndexOfNextRedo(string? shownMapId)
    {
        var isAnotherMapsStepNewer = false;
        for (var index = _undone.Count - 1; index >= 0; index--)
        {
            var step = _undone[index];
            if (step.Target == UndoTarget.Catalogs)
            {
                return isAnotherMapsStepNewer ? -1 : index;
            }

            if (step.MapId == shownMapId)
            {
                return index;
            }

            isAnotherMapsStepNewer = true;
        }

        return -1;
    }

    private static UndoStep Move(List<UndoStep> from, List<UndoStep> to, int index)
    {
        var step = from[index];
        from.RemoveAt(index);
        to.Add(step);
        return step;
    }
}
