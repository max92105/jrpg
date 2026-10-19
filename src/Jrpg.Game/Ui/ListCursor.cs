namespace Jrpg.Game.Ui;

/// <summary>
/// The selected entry of a vertical list. It wraps from one end to the other and passes over disabled entries, such as
/// Continue with no save to continue from.
/// </summary>
internal sealed class ListCursor
{
    private readonly int _count;
    private readonly Func<int, bool> _isEnabled;

    /// <param name="count">How many entries the list has.</param>
    /// <param name="isEnabled">Whether the entry at an index can be selected.</param>
    /// <exception cref="ArgumentException">No entry is enabled, so nothing could be selected.</exception>
    public ListCursor(int count, Func<int, bool> isEnabled)
    {
        _count = count;
        _isEnabled = isEnabled;
        Selected = Enumerable.Range(0, count).FirstOrDefault(isEnabled, -1);
        if (Selected < 0)
        {
            throw new ArgumentException("A list needs at least one enabled entry.", nameof(isEnabled));
        }
    }

    /// <summary>The index of the selected entry; always an enabled one.</summary>
    public int Selected { get; private set; }

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
    }
}
