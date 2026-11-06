using Jrpg.Data.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// A track as the player hears it: the loop file the content names, in a folder of its own under the music folder, and
/// whether an <c>intro.wav</c> and an <c>outro.wav</c> sit beside it (decision 496). A track with either cycles its
/// parts, intro, loop, outro and over, never paused; one without plays its loop looped.
/// </summary>
internal readonly record struct MusicTrack(string Loop, bool HasIntro = false, bool HasOutro = false)
{
    public bool CyclesParts => HasIntro || HasOutro;

    /// <summary>
    /// The files played in turn, each once: the intro when there is one, the loop, the outro when there is one.
    /// </summary>
    public IReadOnlyList<string> Parts
    {
        get
        {
            var parts = new List<string>(3);
            if (HasIntro)
            {
                parts.Add(IntroBeside(Loop));
            }

            parts.Add(Loop);
            if (HasOutro)
            {
                parts.Add(OutroBeside(Loop));
            }

            return parts;
        }
    }

    public static string IntroBeside(string loop) => Beside(loop, AudioFolders.TrackIntro);

    public static string OutroBeside(string loop) => Beside(loop, AudioFolders.TrackOutro);

    // The content names files with '/', whatever the system (AudioFolders.FilesIn).
    private static string Beside(string loop, string name)
    {
        var folderEnd = loop.LastIndexOf('/');
        return folderEnd < 0 ? name : string.Concat(loop.AsSpan(0, folderEnd + 1), name);
    }
}
