namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// The edits done to one draft, for undo and redo, and whether the draft differs from what was last saved.
/// </summary>
internal sealed class EditHistory(MapDraft draft)
{
    private readonly Stack<IMapEdit> _done = new();
    private readonly Stack<IMapEdit> _undone = new();

    // How many edits were done when the draft was last saved; null once that state can't be reached by undo or redo.
    private int? _savedDepth = 0;

    public bool CanUndo => _done.Count > 0;

    public bool CanRedo => _undone.Count > 0;

    /// <summary>Whether the draft differs from the last save; undoing back to the saved state makes it clean again.</summary>
    public bool IsDirty => _savedDepth != _done.Count;

    /// <summary>Applies the edit; any undone edits can no longer be redone.</summary>
    public void Do(IMapEdit edit)
    {
        edit.Apply(draft);
        if (_savedDepth > _done.Count)
        {
            // The saved state was among the undone edits, which are now gone.
            _savedDepth = null;
        }

        _done.Push(edit);
        _undone.Clear();
    }

    public void Undo()
    {
        var edit = _done.Pop();
        edit.Revert(draft);
        _undone.Push(edit);
    }

    public void Redo()
    {
        var edit = _undone.Pop();
        edit.Apply(draft);
        _done.Push(edit);
    }

    public void MarkSaved() => _savedDepth = _done.Count;
}
