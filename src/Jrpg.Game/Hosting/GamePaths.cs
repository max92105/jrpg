using System.Reflection;

namespace Jrpg.Game.Hosting;

/// <summary>Where the game keeps the player's files, outside the install folder so an update never touches them.</summary>
internal static class GamePaths
{
    /// <summary><c>%LocalAppData%/Jrpg/Saves</c> (decision 64).</summary>
    public static string SaveDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jrpg", "Saves");

    /// <summary>
    /// The debug captures: the repository's <c>docs/captures</c>, which a Debug build stamps in so they are committed
    /// and travel between machines; otherwise <c>%LocalAppData%/Jrpg/Captures</c>.
    /// </summary>
    public static string CaptureDirectory { get; } =
        typeof(GamePaths).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(metadata => metadata.Key == "CaptureRoot")?.Value
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jrpg", "Captures");
}
