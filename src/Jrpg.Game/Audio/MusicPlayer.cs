using Jrpg.Data.Audio;
using Microsoft.Xna.Framework.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// Plays on the audio device, on a loop, the track the <see cref="MusicChannel"/> asks for (ADR 0035).
/// </summary>
internal sealed class MusicPlayer(SoundLibrary sounds) : IDisposable
{
    private string? _startedFile;
    private SoundEffectInstance? _playing;

    /// <summary>
    /// Called after every update. A file other than the one last started, or none, stops what plays and starts the new
    /// one from its beginning, a cut (decision 416); the same file plays on. The gain is set on every call, so a change
    /// in the Options page is heard at once.
    /// </summary>
    public void Follow(MusicChannel music, float gain)
    {
        if (music.Track != _startedFile)
        {
            Start(music.Track, gain);
        }
        else if (_playing is not null)
        {
            _playing.Volume = gain;
        }
    }

    public void Dispose() => StopPlaying();

    private void Start(string? file, float gain)
    {
        StopPlaying();
        _startedFile = file;
        if (file is not null && sounds.Get(AudioFolders.Music, file) is { } sound)
        {
            _playing = sound.CreateInstance();
            _playing.IsLooped = true;
            _playing.Volume = gain;
            _playing.Play();
        }
    }

    private void StopPlaying()
    {
        _playing?.Stop();
        _playing?.Dispose();
        _playing = null;
    }
}
