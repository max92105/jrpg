namespace Jrpg.Data.Audio;

/// <summary>Where sound lives: <c>{content}/audio/</c>, one folder per kind (ADR 0018).</summary>
public static class AudioFolders
{
    /// <summary>Voice-overs, started by voice steps.</summary>
    public const string Voices = "voices";

    /// <summary>Music: map tracks, and the themes the code names.</summary>
    public const string Music = "music";

    /// <summary>Sound effects: the ones the code names and each skill's.</summary>
    public const string Effects = "effects";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "audio");

    /// <returns>The <c>.wav</c> names in a folder, by exact name, sorted; empty when the folder is missing.</returns>
    public static IReadOnlyList<string> FilesIn(string audioRoot, string folder)
    {
        var path = Path.Combine(audioRoot, folder);
        return Directory.Exists(path)
            ? [.. Directory.EnumerateFiles(path, "*.wav").Select(file => Path.GetFileName(file)).Order(StringComparer.Ordinal)]
            : [];
    }
}
