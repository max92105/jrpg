using Jrpg.Data.Audio;
using Microsoft.Xna.Framework.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// Plays on the audio device what the <see cref="MusicMix"/> makes of the <see cref="MusicChannel"/> (ADR 0035): each
/// track's loop on a looped instance, after its intro when it has one, and its outro once when it leaves. A track that
/// leaves the mix is released, its instance and its loaded loop, unless the host keeps it loaded; intros and outros
/// stay loaded, being short.
/// </summary>
/// <param name="sounds">Opens the tracks.</param>
/// <param name="keptLoaded">The loops never released: the themes preloaded so a flash doesn't stall on a load.</param>
internal sealed class MusicPlayer(SoundLibrary sounds, IReadOnlyList<string> keptLoaded) : IDisposable
{
    private readonly MusicMix _mix = new();

    // By loop file: the track's loop, or its intro while that plays.
    private readonly Dictionary<string, SoundEffectInstance> _instances = [];
    private readonly List<string> _inIntro = [];
    private readonly List<SoundEffectInstance> _outros = [];

    /// <summary>
    /// Called after every update, with the Music volume's gain, so a change in the Options page is heard at once.
    /// </summary>
    public void Follow(MusicChannel music, float gain, TimeSpan delta)
    {
        var (wasPlaying, wasLeaving, wasPaused) = (_mix.Playing, _mix.Leaving, _mix.Paused);
        _mix.Want(Wanted(music.Track));
        _mix.Advance(delta);
        Settle(wasPlaying, gain);
        Settle(wasLeaving, gain);
        Settle(wasPaused, gain);
        if (_mix.Playing is { } playing && playing != wasPlaying)
        {
            Begin(playing, gain * _mix.PlayingGain);
        }

        SetVolume(_mix.Playing, gain * _mix.PlayingGain);
        SetVolume(_mix.Leaving, gain * _mix.LeavingGain);
        FollowIntros(gain);
        FollowOutros(gain);
    }

    public void Dispose()
    {
        foreach (var instance in _instances.Values)
        {
            instance.Stop();
            instance.Dispose();
        }

        foreach (var outro in _outros)
        {
            outro.Stop();
            outro.Dispose();
        }

        _instances.Clear();
        _inIntro.Clear();
        _outros.Clear();
    }

    // The flow names the loop file; whether an intro and an outro sit beside it is read once, when the track changes.
    private MusicTrack? Wanted(string? loop)
    {
        if (loop is null)
        {
            return null;
        }

        if (_mix.Playing is { } playing && playing.Loop == loop)
        {
            return playing;
        }

        return new MusicTrack(
            loop,
            sounds.Exists(AudioFolders.Music, MusicTrack.IntroBeside(loop)),
            sounds.Exists(AudioFolders.Music, MusicTrack.OutroBeside(loop)));
    }

    // A track gone from the mix ends (its outro, or dropped); one whose fade-out ended and that waits is paused.
    private void Settle(MusicTrack? was, float gain)
    {
        if (was is not { } track)
        {
            return;
        }

        if (track != _mix.Playing && track != _mix.Leaving && track != _mix.Paused)
        {
            End(track, gain);
        }
        else if (track == _mix.Paused && track != _mix.Leaving && _instances.TryGetValue(track.Loop, out var waiting) && waiting.State == SoundState.Playing)
        {
            waiting.Pause();
        }
    }

    // A track waiting paused resumes where it was; any other starts from its beginning, intro first when it has one.
    private void Begin(MusicTrack track, float volume)
    {
        if (_instances.TryGetValue(track.Loop, out var waiting))
        {
            waiting.Volume = volume;
            waiting.Resume();
        }
        else if (track.HasIntro && Start(MusicTrack.IntroBeside(track.Loop), volume) is { } intro)
        {
            _instances.Add(track.Loop, intro);
            _inIntro.Add(track.Loop);
        }
        else if (Start(track.Loop, volume) is { } loop)
        {
            // Reached with no intro, or with one whose file won't load: the loop is better than silence.
            loop.IsLooped = true;
            _instances.Add(track.Loop, loop);
        }
    }

    private void End(MusicTrack track, float gain)
    {
        if (!_instances.Remove(track.Loop, out var instance))
        {
            return;
        }

        instance.Stop();
        instance.Dispose();
        _inIntro.Remove(track.Loop);
        if (!keptLoaded.Contains(track.Loop))
        {
            sounds.Release(AudioFolders.Music, track.Loop);
        }

        if (track.HasOutro && Start(MusicTrack.OutroBeside(track.Loop), gain) is { } outro)
        {
            _outros.Add(outro);
        }
    }

    // An intro that reached its end hands over to the loop, while the track is still the one playing.
    private void FollowIntros(float gain)
    {
        for (var i = _inIntro.Count - 1; i >= 0; i--)
        {
            var loop = _inIntro[i];
            if (_instances[loop].State != SoundState.Stopped)
            {
                continue;
            }

            _instances[loop].Dispose();
            _instances.Remove(loop);
            _inIntro.RemoveAt(i);
            if (_mix.Playing?.Loop == loop && Start(loop, gain * _mix.PlayingGain) is { } started)
            {
                started.IsLooped = true;
                _instances.Add(loop, started);
            }
        }
    }

    private void FollowOutros(float gain)
    {
        for (var i = _outros.Count - 1; i >= 0; i--)
        {
            if (_outros[i].State == SoundState.Stopped)
            {
                _outros[i].Dispose();
                _outros.RemoveAt(i);
            }
            else
            {
                _outros[i].Volume = gain;
            }
        }
    }

    private void SetVolume(MusicTrack? track, float volume)
    {
        if (track is { } playing && _instances.TryGetValue(playing.Loop, out var instance))
        {
            instance.Volume = volume;
        }
    }

    private SoundEffectInstance? Start(string file, float volume)
    {
        if (sounds.Get(AudioFolders.Music, file) is not { } sound)
        {
            return null;
        }

        var started = sound.CreateInstance();
        started.Volume = volume;
        started.Play();
        return started;
    }
}
