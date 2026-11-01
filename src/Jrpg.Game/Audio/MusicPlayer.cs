using Jrpg.Data.Audio;
using Microsoft.Xna.Framework.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// Plays on the audio device, on a loop, what the <see cref="MusicMix"/> makes of the <see cref="MusicChannel"/> (ADR
/// 0035): at most two instances, the one playing and the one waiting paused. A track that leaves the mix is released,
/// its instance and its loaded sound, unless the host keeps it loaded.
/// </summary>
/// <param name="sounds">Opens the tracks.</param>
/// <param name="keptLoaded">The tracks never released: the themes preloaded so a flash doesn't stall on a load.</param>
internal sealed class MusicPlayer(SoundLibrary sounds, IReadOnlyList<string> keptLoaded) : IDisposable
{
    private readonly MusicMix _mix = new();
    private readonly Dictionary<string, SoundEffectInstance> _instances = [];

    /// <summary>
    /// Called after every update, with the Music volume's gain, so a change in the Options page is heard at once.
    /// </summary>
    public void Follow(MusicChannel music, float gain, TimeSpan delta)
    {
        var (wasPlaying, wasPaused) = (_mix.Playing, _mix.Paused);
        _mix.Want(music.Track);
        _mix.Advance(delta);
        if (_mix.Playing != wasPlaying)
        {
            ApplyChange(wasPlaying, wasPaused, gain * _mix.PlayingGain);
        }

        if (_mix.Playing is { } playing && _instances.TryGetValue(playing, out var instance))
        {
            instance.Volume = gain * _mix.PlayingGain;
        }
    }

    public void Dispose()
    {
        foreach (var instance in _instances.Values)
        {
            instance.Stop();
            instance.Dispose();
        }

        _instances.Clear();
    }

    private void ApplyChange(string? wasPlaying, string? wasPaused, float volume)
    {
        foreach (var dropped in TracksLeavingTheMix(wasPlaying, wasPaused))
        {
            Drop(dropped);
        }

        if (_mix.Paused is { } paused && paused != wasPaused && _instances.TryGetValue(paused, out var leaving))
        {
            leaving.Pause();
        }

        if (_mix.Playing is { } playing && playing != wasPlaying)
        {
            Begin(playing, volume);
        }
    }

    private IEnumerable<string> TracksLeavingTheMix(string? wasPlaying, string? wasPaused) =>
        new[] { wasPlaying, wasPaused }.OfType<string>().Where(file => file != _mix.Playing && file != _mix.Paused);

    // A track waiting paused resumes where it was; any other starts from its beginning.
    private void Begin(string file, float volume)
    {
        if (_instances.TryGetValue(file, out var waiting))
        {
            waiting.Volume = volume;
            waiting.Resume();
        }
        else if (sounds.Get(AudioFolders.Music, file) is { } sound)
        {
            var started = sound.CreateInstance();
            started.IsLooped = true;
            started.Volume = volume;
            started.Play();
            _instances.Add(file, started);
        }
    }

    private void Drop(string file)
    {
        if (_instances.Remove(file, out var instance))
        {
            instance.Stop();
            instance.Dispose();
        }

        if (!keptLoaded.Contains(file))
        {
            sounds.Release(AudioFolders.Music, file);
        }
    }
}
