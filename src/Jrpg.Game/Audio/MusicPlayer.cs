using Jrpg.Data.Audio;
using Microsoft.Xna.Framework.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// Plays on the audio device what the <see cref="MusicMix"/> makes of the <see cref="MusicChannel"/> (ADR 0035): a
/// track's loop on a looped instance, or its parts one after the other and over when it has an intro or an outro. A
/// track that leaves the mix is released, its instance and its loaded loop, unless the host keeps it loaded; intros and
/// outros stay loaded, being short.
/// </summary>
/// <param name="sounds">Opens the tracks.</param>
/// <param name="keptLoaded">The loops never released: the themes preloaded so a flash doesn't stall on a load.</param>
internal sealed class MusicPlayer(SoundLibrary sounds, IReadOnlyList<string> keptLoaded) : IDisposable
{
    private readonly MusicMix _mix = new();

    // By loop file: the instance heard for the track, its current part for one that cycles.
    private readonly Dictionary<string, SoundEffectInstance> _instances = [];
    private readonly Dictionary<string, int> _partPlaying = [];
    private readonly List<string> _cycling = [];

    /// <summary>
    /// Called after every update, with the Music volume's gain, so a change in the Options page is heard at once.
    /// </summary>
    public void Follow(MusicChannel music, float gain, TimeSpan delta)
    {
        var (wasPlaying, wasLeaving, wasPaused) = (_mix.Playing, _mix.Leaving, _mix.Paused);
        _mix.Want(Wanted(music.Track));
        _mix.Advance(delta);
        Settle(wasPlaying);
        Settle(wasLeaving);
        Settle(wasPaused);
        if (_mix.Playing is { } playing && playing != wasPlaying)
        {
            Begin(playing, gain * _mix.PlayingGain);
        }

        FollowParts(gain);
        SetVolume(_mix.Playing, gain * _mix.PlayingGain);
        SetVolume(_mix.Leaving, gain * _mix.LeavingGain);
    }

    public void Dispose()
    {
        foreach (var instance in _instances.Values)
        {
            instance.Stop();
            instance.Dispose();
        }

        _instances.Clear();
        _partPlaying.Clear();
        _cycling.Clear();
    }

    // The flow names the loop file; whether an intro and an outro sit beside it is read once, on the frame the track is
    // first wanted: from then on the mix knows it, as the next track, the playing one, the leaving or the paused one.
    private MusicTrack? Wanted(string? loop)
    {
        if (loop is null)
        {
            return null;
        }

        if (TrackOf(loop) is { } known)
        {
            return known;
        }

        return new MusicTrack(
            loop,
            sounds.Exists(AudioFolders.Music, MusicTrack.IntroBeside(loop)),
            sounds.Exists(AudioFolders.Music, MusicTrack.OutroBeside(loop)));
    }

    private MusicTrack? TrackOf(string loop)
    {
        if (_mix.Next is { } next && next.Loop == loop)
        {
            return next;
        }

        if (_mix.Leaving is { } leaving && leaving.Loop == loop)
        {
            return leaving;
        }

        return _mix.Paused is { } paused && paused.Loop == loop ? paused : null;
    }

    // A track gone from the mix is dropped; one whose fade-out ended and that waits is paused.
    private void Settle(MusicTrack? was)
    {
        if (was is not { } track)
        {
            return;
        }

        if (track != _mix.Playing && track != _mix.Leaving && track != _mix.Paused)
        {
            Drop(track);
        }
        else if (track == _mix.Paused && track != _mix.Leaving && _instances.TryGetValue(track.Loop, out var waiting) && waiting.State == SoundState.Playing)
        {
            waiting.Pause();
        }
    }

    // A track waiting paused resumes where it was; any other starts from its beginning, its first part when it cycles.
    private void Begin(MusicTrack track, float volume)
    {
        if (_instances.TryGetValue(track.Loop, out var waiting))
        {
            waiting.Volume = volume;
            waiting.Resume();
        }
        else if (track.CyclesParts && StartPart(track, 0, volume))
        {
            _cycling.Add(track.Loop);
        }
        else if (StartLooped(track.Loop, volume) is { } loop)
        {
            _instances.Add(track.Loop, loop);
        }
    }

    private void Drop(MusicTrack track)
    {
        if (_instances.Remove(track.Loop, out var instance))
        {
            instance.Stop();
            instance.Dispose();
        }

        _cycling.Remove(track.Loop);
        _partPlaying.Remove(track.Loop);
        if (!keptLoaded.Contains(track.Loop))
        {
            sounds.Release(AudioFolders.Music, track.Loop);
        }
    }

    // A part that reached its end hands over to the next, the last to the first again.
    private void FollowParts(float gain)
    {
        for (var i = _cycling.Count - 1; i >= 0; i--)
        {
            var loop = _cycling[i];
            if (!_instances.TryGetValue(loop, out var part) || part.State != SoundState.Stopped || TrackOf(loop) is not { } track)
            {
                continue;
            }

            part.Dispose();
            _instances.Remove(loop);
            var volume = gain * (track == _mix.Playing ? _mix.PlayingGain : _mix.LeavingGain);
            StartPart(track, _partPlaying[loop] + 1, volume);
        }
    }

    // A part that won't load is skipped for the next, so a bad intro still leaves the loop heard; none starting, false.
    private bool StartPart(MusicTrack track, int index, float volume)
    {
        var parts = track.Parts;
        for (var tried = 0; tried < parts.Count; tried++)
        {
            var playing = (index + tried) % parts.Count;
            if (StartOnce(parts[playing], volume) is { } part)
            {
                _instances[track.Loop] = part;
                _partPlaying[track.Loop] = playing;
                return true;
            }
        }

        return false;
    }

    private void SetVolume(MusicTrack? track, float volume)
    {
        if (track is { } heard && _instances.TryGetValue(heard.Loop, out var instance))
        {
            instance.Volume = volume;
        }
    }

    private SoundEffectInstance? StartLooped(string file, float volume)
    {
        var instance = Prepare(file, volume);
        if (instance is not null)
        {
            // Set before Play: set after, the device may keep the one-shot it was given.
            instance.IsLooped = true;
            instance.Play();
        }

        return instance;
    }

    private SoundEffectInstance? StartOnce(string file, float volume)
    {
        var instance = Prepare(file, volume);
        instance?.Play();
        return instance;
    }

    private SoundEffectInstance? Prepare(string file, float volume)
    {
        if (sounds.Get(AudioFolders.Music, file) is not { } sound)
        {
            return null;
        }

        var instance = sound.CreateInstance();
        instance.Volume = volume;
        return instance;
    }
}
