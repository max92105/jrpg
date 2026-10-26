namespace Jrpg.Game.Ui;

/// <summary>
/// Where a list's highlight is while it glides from the row it left to the selected one (decision 262): 0.08 s, easing
/// out, so moving the cursor reads smoothly but never lags. A move that wraps around the list jumps instead.
/// </summary>
/// <param name="row">The row selected at first.</param>
internal sealed class SelectionGlide(int row)
{
    private static readonly TimeSpan _duration = TimeSpan.FromSeconds(0.08);

    private float _from = row;
    private int _to = row;
    private TimeSpan _elapsed = _duration;

    /// <summary>The highlight's place, in rows: a whole number once it has arrived.</summary>
    public float Position => _from + ((_to - _from) * Easing.CubicOut(Easing.ShareOf(_elapsed, _duration)));

    /// <summary>Glides to <paramref name="row"/> from wherever the highlight is now, even mid-glide.</summary>
    public void Select(int row)
    {
        _from = Position;
        _to = row;
        _elapsed = TimeSpan.Zero;
    }

    /// <summary>Puts the highlight on <paramref name="row"/> at once, as when the cursor wraps around.</summary>
    public void JumpTo(int row)
    {
        _from = row;
        _to = row;
        _elapsed = _duration;
    }

    /// <summary>
    /// Follows the cursor to <paramref name="row"/>: nothing when it hasn't moved, a jump when it wrapped around the
    /// list (it moved more than half the list), a glide otherwise, a skipped disabled row included. Callers run it
    /// after each update's input.
    /// </summary>
    /// <param name="row">The cursor's row now.</param>
    /// <param name="rowCount">How many rows the list has.</param>
    public void Follow(int row, int rowCount)
    {
        if (row == _to)
        {
            return;
        }

        if (Math.Abs(row - _to) > rowCount / 2)
        {
            JumpTo(row);
            return;
        }

        Select(row);
    }

    public void Advance(TimeSpan delta) => _elapsed += delta;
}
