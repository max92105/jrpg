using Jrpg.Data.Audio;
using Microsoft.Xna.Framework.Audio;

namespace Jrpg.Game.Audio;

/// <summary>Plays on the audio device the voice-over the <see cref="VoiceChannel"/> asks for (ADR 0018).</summary>
internal sealed class VoicePlayer(SoundLibrary sounds) : IDisposable
{
    private string? _startedFile;
    private SoundEffectInstance? _playing;

    /// <summary>
    /// Called after every update. A file other than the one last started, or none, stops what plays and starts the new
    /// one from its beginning; the same file plays on. A stop and a replay of the same file come in different updates,
    /// so the replay starts over.
    /// </summary>
    public void Follow(VoiceChannel voice)
    {
        if (voice.VoiceFile == _startedFile)
        {
            return;
        }

        StopPlaying();
        _startedFile = voice.VoiceFile;
        if (_startedFile is not null && sounds.Get(AudioFolders.Voices, _startedFile) is { } sound)
        {
            _playing = sound.CreateInstance();
            _playing.Play();
        }
    }

    public void Dispose() => StopPlaying();

    private void StopPlaying()
    {
        _playing?.Stop();
        _playing?.Dispose();
        _playing = null;
    }
}
