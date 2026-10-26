namespace Jrpg.Game.Ui;

/// <summary>
/// Where a list's visible window starts: it moves only as far as it must to keep one entry in view, so it scrolls an
/// entry at a time and jumps only when the entry wraps around the list. Shared by every scrolling list.
/// </summary>
internal static class ScrollWindow
{
    /// <summary>The window's first entry after <paramref name="index"/> is brought into view.</summary>
    /// <param name="firstVisible">The window's first entry now.</param>
    /// <param name="index">The entry to keep in view.</param>
    /// <param name="count">How many entries the list has.</param>
    /// <param name="visibleCount">How many entries the window shows.</param>
    public static int KeepInView(int firstVisible, int index, int count, int visibleCount)
    {
        var first = Clamp(firstVisible, count, visibleCount);
        if (index < first)
        {
            return index;
        }

        return index >= first + visibleCount ? index - visibleCount + 1 : first;
    }

    /// <summary>The window's first entry, moved up if the list got shorter so the window never runs past it.</summary>
    /// <param name="firstVisible">The window's first entry now.</param>
    /// <param name="count">How many entries the list has.</param>
    /// <param name="visibleCount">How many entries the window shows.</param>
    public static int Clamp(int firstVisible, int count, int visibleCount) =>
        Math.Min(firstVisible, Math.Max(0, count - visibleCount));
}
