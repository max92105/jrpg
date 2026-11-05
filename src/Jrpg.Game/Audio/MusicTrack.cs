using Jrpg.Data.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// A track as the player hears it: the loop file the content names, in its own folder under the music folder, and
/// whether an <c>intro.wav</c> and an <c>outro.wav</c> sit beside it (decision 496). A track with them opens with the
/// intro, loops, and ends with the outro; one without is faded in and out by the <see cref="MusicMix"/>.
/// </summary>
internal readonly record struct MusicTrack(string Loop, bool HasIntro = false, bool HasOutro = false)
{
    public static string IntroBeside(string loop) => Beside(loop, AudioFolders.TrackIntro);

    public static string OutroBeside(string loop) => Beside(loop, AudioFolders.TrackOutro);

    // The content names files with '/', whatever the system (AudioFolders.FilesIn).
    private static string Beside(string loop, string name)
    {
        var folderEnd = loop.LastIndexOf('/');
        return folderEnd < 0 ? name : string.Concat(loop.AsSpan(0, folderEnd + 1), name);
    }
}
