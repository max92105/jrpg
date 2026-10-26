namespace Jrpg.Game.Ui;

/// <summary>
/// How a list that shows only some of its entries scrolls: as little as it must to keep one entry in view, so it moves
/// a row at a time and jumps only when that entry does.
/// </summary>
internal static class ListScroll
{
    /// <param name="firstVisible">The first entry in view now.</param>
    /// <param name="index">The entry to keep in view.</param>
    /// <param name="visibleCount">How many entries show at once.</param>
    /// <returns>The first entry in view once <paramref name="index"/> is in it.</returns>
    public static int KeepInView(int firstVisible, int index, int visibleCount)
    {
        if (index < firstVisible)
        {
            return index;
        }

        return index >= firstVisible + visibleCount ? index - visibleCount + 1 : firstVisible;
    }
}
