using Jrpg.Data.Content;

namespace Jrpg.Data.Audio;

/// <summary>Where sound lives: <c>{content}/audio/</c>, one folder per kind (ADR 0018).</summary>
public static class AudioFolders
{
    /// <summary>Voice-overs, started by voice steps.</summary>
    public const string Voices = "voices";

    /// <summary>
    /// Music: map tracks, and the themes the code names, each in its own folder as <c>town/loop.wav</c>, with an
    /// <c>intro.wav</c> and an <c>outro.wav</c> beside it when the track has them (decision 496).
    /// </summary>
    public const string Music = "music";

    /// <summary>Sound effects: the ones the code names and each skill's.</summary>
    public const string Effects = "effects";

    /// <summary>The files the player finds beside a track's loop, never named by content (decision 496).</summary>
    public const string TrackIntro = "intro.wav";

    public const string TrackOutro = "outro.wav";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "audio");

    /// <returns>
    /// The <c>.wav</c> names in a folder and its subfolders, by exact name under the folder with <c>/</c>, sorted;
    /// empty when the folder is missing.
    /// </returns>
    public static IReadOnlyList<string> FilesIn(string audioRoot, string folder)
    {
        var path = Path.Combine(audioRoot, folder);
        return Directory.Exists(path)
            ? [.. Directory.EnumerateFiles(path, "*.wav", SearchOption.AllDirectories).Select(file => ExactFileName.NameUnder(path, file)).Order(StringComparer.Ordinal)]
            : [];
    }

    /// <returns>The tracks the content may name: the music files but a track's intro and outro.</returns>
    public static IReadOnlyList<string> TracksIn(string audioRoot) =>
        [.. FilesIn(audioRoot, Music).Where(file => Path.GetFileName(file) is not (TrackIntro or TrackOutro))];
}
