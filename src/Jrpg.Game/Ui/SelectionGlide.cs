using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// Where a list's highlight is while it glides from the row it left to the selected one (decision 262): 0.08 s, easing
/// out, so moving the cursor reads smoothly but never lags. A move that wraps around the list jumps instead. It also
/// keeps the time for the list pointer's bob.
/// </summary>
/// <param name="row">The row selected at first.</param>
internal sealed class SelectionGlide(int row)
{
    private static readonly TimeSpan _duration = TimeSpan.FromSeconds(0.08);

    // The list pointer bobs 4 px left and back (decision 262).
    private const float PointerBobDistance = 4;

    private float _from = row;
    private int _to = row;
    private TimeSpan _elapsed = _duration;
    private TimeSpan _bobElapsed;

    /// <summary>The highlight's place, in rows: a whole number once it has arrived.</summary>
    public float Position => _from + ((_to - _from) * Easing.CubicOut(Easing.ShareOf(_elapsed, _duration)));

    /// <summary>The highlight's band in a list of evenly spaced rows.</summary>
    /// <param name="firstRow">The first visible row; the others follow it one height apart.</param>
    public Rectangle BandOver(Rectangle firstRow) => firstRow with { Y = firstRow.Y + (int)Math.Round(Position * firstRow.Height) };

    /// <summary>Where the list pointer goes on <paramref name="band"/>: at its left middle, bobbed left.</summary>
    public Vector2 PointerAt(Rectangle band) => new(band.X - PointerBob.OffsetAt(_bobElapsed, PointerBobDistance), band.Center.Y);

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

    /// <summary>
    /// Follows a scrolling list's cursor by its row in the visible window, so a scroll that keeps the cursor on the
    /// same screen row leaves the highlight still.
    /// </summary>
    public void FollowShown(ListCursor cursor) => Follow(cursor.Selected - cursor.FirstVisible, cursor.WindowSize);

    public void Advance(TimeSpan delta)
    {
        _elapsed += delta;
        _bobElapsed += delta;
    }
}
