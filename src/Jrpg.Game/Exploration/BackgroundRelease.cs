using Jrpg.Game.Ui;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Which of the last map's battle backgrounds to release on entering another (decision 900), so video memory holds one
/// map's backgrounds, not every one visited.
/// </summary>
internal static class BackgroundRelease
{
    /// <param name="previous">The backgrounds the last map's battles could show; empty on the first map.</param>
    /// <param name="next">The backgrounds the entered map's battles can show.</param>
    /// <returns>
    /// The files in <paramref name="previous"/> that <paramref name="next"/> doesn't hold, never the title's or the
    /// menu's: the menu's 200 ms decode is done once at startup on purpose (<c>JrpgGame.LoadContent</c>).
    /// </returns>
    public static IReadOnlyList<string> Of(IReadOnlyList<string> previous, IReadOnlyList<string> next) =>
        previous.Where(file => !next.Contains(file) && !IsScreenBackground(file)).ToList();

    private static bool IsScreenBackground(string file) =>
        file is UiArtFiles.Backgrounds.Title or UiArtFiles.Backgrounds.Menu;
}
