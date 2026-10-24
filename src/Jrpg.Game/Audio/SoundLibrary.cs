using Jrpg.Game.Hosting;
using Microsoft.Xna.Framework.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// The one place the game opens sound files: each is loaded on first use and kept. A file that won't load is reported
/// once, by name, and plays silent (ADR 0018).
/// </summary>
internal sealed class SoundLibrary(string audioRoot) : IDisposable
{
    // Null for a file that failed, so it isn't tried and reported again.
    private readonly Dictionary<string, SoundEffect?> _loaded = [];

    /// <returns>The sound, or null when its file couldn't be loaded.</returns>
    public SoundEffect? Get(string folder, string file)
    {
        var path = Path.Combine(audioRoot, folder, file);
        if (!_loaded.TryGetValue(path, out var sound))
        {
            sound = TryLoad(path);
            _loaded.Add(path, sound);
        }

        return sound;
    }

    public void Dispose()
    {
        foreach (var sound in _loaded.Values)
        {
            sound?.Dispose();
        }

        _loaded.Clear();
    }

    // The startup check found the file, so what fails here is its contents: not a WAV, or an encoding MonoGame lacks.
    private static SoundEffect? TryLoad(string path)
    {
        try
        {
            return SoundEffect.FromFile(path);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            DeveloperLog.Report($"Can't play {path}: {exception.Message} It stays silent.");
            return null;
        }
    }
}
