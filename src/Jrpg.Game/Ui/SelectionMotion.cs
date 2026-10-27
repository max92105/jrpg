using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// A scrolling list's selection in motion, the same on every page list (decision 262): the highlight glides between the
/// rows of the visible window and the pointer bobs. It follows the cursor's row in the window, so a scroll that keeps
/// that row in place doesn't glide, and a wrap to the other end jumps.
/// </summary>
internal sealed class SelectionMotion
{
    private readonly SelectionGlide _glide = new(0);

    // How long the list has shown, for the pointer's bob.
    private TimeSpan _shown;

    /// <summary>The highlight's place, in rows from the top of the visible window.</summary>
    public float Position => _glide.Position;

    /// <summary>How far left of its place the pointer has bobbed, in pixels.</summary>
    public float PointerOffset => PointerBob.OffsetAt(_shown, MenuPointer.BobDistance);

    /// <summary>The selection in a list of rows <paramref name="firstRow"/>'s height, one under another.</summary>
    /// <param name="firstRow">The visible window's top row.</param>
    public GlidingRow RowIn(Rectangle firstRow) =>
        At(firstRow with { Y = firstRow.Y + (int)Math.Round(Position * firstRow.Height) });

    /// <summary>The selection drawn at <paramref name="row"/>, for a list whose rows aren't evenly spaced.</summary>
    /// <param name="row">Where the list places <see cref="Position"/>.</param>
    public GlidingRow At(Rectangle row) => new(row, PointerOffset);

    public void Advance(TimeSpan delta)
    {
        _glide.Advance(delta);
        _shown += delta;
    }

    /// <summary>Glides to the cursor's row in the window; callers run it after each update's input.</summary>
    public void Follow(ListCursor cursor) => Follow(cursor.VisibleRow, cursor.VisibleCount);

    /// <summary>Glides to <paramref name="visibleRow"/>, for a list without a <see cref="ListCursor"/>.</summary>
    /// <param name="visibleRow">The selected row, from the top of the visible window.</param>
    /// <param name="visibleCount">How many rows the window shows; a move of more than half of them jumps.</param>
    public void Follow(int visibleRow, int visibleCount) => _glide.Follow(visibleRow, visibleCount);

    /// <summary>Puts the highlight on the cursor's row at once, as when the list is rebuilt.</summary>
    public void JumpTo(ListCursor cursor) => JumpTo(cursor.VisibleRow);

    /// <summary>Puts the highlight on <paramref name="visibleRow"/> at once.</summary>
    public void JumpTo(int visibleRow) => _glide.JumpTo(visibleRow);
}
