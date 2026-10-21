namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The catalog edits done, for undo and redo, and whether the catalogs differ from what was last saved, as
/// <c>EditHistory</c> does for a map.
/// </summary>
internal sealed class CatalogHistory
{
    private readonly Stack<CatalogEdit> _done = new();
    private readonly Stack<CatalogEdit> _undone = new();

    // How many edits were done when the catalogs were last saved; null once that state can't be reached by undo or redo.
    private int? _savedDepth = 0;

    public bool CanUndo => _done.Count > 0;

    public bool CanRedo => _undone.Count > 0;

    public bool IsDirty => _savedDepth != _done.Count;

    /// <summary>The edit the next undo reverts; null when there is none.</summary>
    public CatalogEdit? NextUndo => _done.Count > 0 ? _done.Peek() : null;

    /// <summary>Records an edit already applied; any undone edits can no longer be redone.</summary>
    public void Push(CatalogEdit appliedEdit)
    {
        if (_savedDepth > _done.Count)
        {
            // The saved state was among the undone edits, which are now gone.
            _savedDepth = null;
        }

        _done.Push(appliedEdit);
        _undone.Clear();
    }

    /// <returns>The edit to revert.</returns>
    public CatalogEdit Undo()
    {
        var edit = _done.Pop();
        _undone.Push(edit);
        return edit;
    }

    /// <returns>The edit to apply again.</returns>
    public CatalogEdit Redo()
    {
        var edit = _undone.Pop();
        _done.Push(edit);
        return edit;
    }

    /// <summary>
    /// A change no undo takes back (a tile made by painting) was made: no undo or redo reaches the saved state now, and an
    /// undone edit can't be redone over it.
    /// </summary>
    public void RecordChangeOutsideHistory()
    {
        _savedDepth = null;
        _undone.Clear();
    }

    public void MarkSaved() => _savedDepth = _done.Count;
}
