namespace Jrpg.Game.Hosting;

/// <summary>Where the game keeps the player's files, outside the install folder so an update never touches them.</summary>
internal static class GamePaths
{
    /// <summary><c>%LocalAppData%/Jrpg/Saves</c> (decision 64).</summary>
    public static string SaveDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jrpg", "Saves");
}
