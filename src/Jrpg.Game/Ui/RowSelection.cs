namespace Jrpg.Game.Ui;

/// <summary>
/// A rebuilt list's selection: the scrolling cursor over its rows (null while it has none) and the highlight gliding to
/// the cursor's row (decision 262). The list resets it when its rows change, keeping the row or the last one.
/// </summary>
internal sealed class RowSelection
{
    /// <summary>The cursor over the rows; null when there is none.</summary>
    public ListCursor? Cursor { get; private set; }

    /// <summary>The highlight gliding to the cursor's row.</summary>
    public SelectionMotion Highlight { get; } = new();

    /// <summary>The selected row's index; null when there is none.</summary>
    public int? SelectedIndex => Cursor?.Selected;

    public void Advance(TimeSpan delta) => Highlight.Advance(delta);

    /// <summary>
    /// A cursor over <paramref name="count"/> rows at <paramref name="selected"/>, or the last row once the list got
    /// shorter, with the highlight jumped there; no cursor at all when the list is empty.
    /// </summary>
    public void Reset(int count, int selected, int visibleRows)
    {
        if (count == 0)
        {
            Cursor = null;
            return;
        }

        Cursor = new ListCursor(count, _ => true, visibleRows);
        Cursor.Select(selected);
        Highlight.JumpTo(Cursor);
    }

    public void MoveUp() => Move(cursor => cursor.MoveUp());

    public void MoveDown() => Move(cursor => cursor.MoveDown());

    private void Move(Action<ListCursor> step)
    {
        if (Cursor is not { } cursor)
        {
            return;
        }

        step(cursor);
        Highlight.Follow(cursor);
    }
}
