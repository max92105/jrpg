namespace Jrpg.Game.Ui;

/// <summary>
/// The selected entry of a vertical list. It wraps from one end to the other and passes over disabled entries, such as
/// a member an item can't be used on.
/// </summary>
internal sealed class ListCursor
{
    private readonly int _count;
    private readonly Func<int, bool> _isEnabled;
    private readonly int _visibleRows;

    /// <param name="count">How many entries the list has.</param>
    /// <param name="isEnabled">Whether the entry at an index can be selected.</param>
    /// <exception cref="ArgumentException">No entry is enabled, so nothing could be selected.</exception>
    public ListCursor(int count, Func<int, bool> isEnabled)
        : this(count, isEnabled, count)
    {
    }

    /// <param name="count">How many entries the list has.</param>
    /// <param name="isEnabled">Whether the entry at an index can be selected.</param>
    /// <param name="visibleRows">How many entries show at once; the window scrolls to keep the selected one in it.</param>
    /// <exception cref="ArgumentException">No entry is enabled, so nothing could be selected.</exception>
    public ListCursor(int count, Func<int, bool> isEnabled, int visibleRows)
    {
        _count = count;
        _visibleRows = visibleRows;
        _isEnabled = isEnabled;
        Selected = Enumerable.Range(0, count).FirstOrDefault(isEnabled, -1);
        if (Selected < 0)
        {
            throw new ArgumentException("A list needs at least one enabled entry.", nameof(isEnabled));
        }

        ScrollToSelected();
    }

    /// <summary>The index of the selected entry; always an enabled one.</summary>
    public int Selected { get; private set; }

    /// <summary>The index of the first entry in the visible window.</summary>
    public int FirstVisible { get; private set; }

    /// <summary>Selects the entry at <paramref name="index"/>, or the last one when the list got shorter; it must be enabled.</summary>
    /// <exception cref="ArgumentException">That entry is disabled.</exception>
    public void Select(int index)
    {
        var clamped = Math.Clamp(index, 0, _count - 1);
        if (!_isEnabled(clamped))
        {
            throw new ArgumentException($"Entry {clamped} is disabled.", nameof(index));
        }

        Selected = clamped;
        ScrollToSelected();
    }

    public void MoveUp() => Step(-1);

    public void MoveDown() => Step(1);

    private void Step(int direction)
    {
        var index = Selected;
        do
        {
            index = (index + direction + _count) % _count;
        }
        while (!_isEnabled(index));

        Selected = index;
        ScrollToSelected();
    }

    // The window moves only as far as it must, so it scrolls a row at a time and jumps only when the cursor wraps.
    private void ScrollToSelected()
    {
        if (Selected < FirstVisible)
        {
            FirstVisible = Selected;
        }
        else if (Selected >= FirstVisible + _visibleRows)
        {
            FirstVisible = Selected - _visibleRows + 1;
        }
    }
}
