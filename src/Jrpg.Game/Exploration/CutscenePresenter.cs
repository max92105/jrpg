using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Audio;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Shows the running event's cutscene for <see cref="EventPresenter"/>: a cinematic step plays its record's stills with
/// their voices, its pictures loaded first, black before and after, and only a held Confirm skips it (ADR 0046,
/// decision 623). The event's voice-over plays through the voice channel until the event ends, is skipped (ADR 0018) or
/// a cinematic cuts it for the rest of the event (decision 622).
/// </summary>
/// <param name="exploration">The running event, whose stills and voice show.</param>
/// <param name="view">Draws a still with its subtitle and the skip hint.</param>
/// <param name="voice">Where the event's voice-over is asked for.</param>
internal sealed class CutscenePresenter(ExplorationState exploration, StillView view, VoiceChannel voice)
{
    private readonly CinematicRun _cinematic = new(exploration, view);

    // The voice step a cinematic of the running event cut (decision 622), so it doesn't come back; null otherwise.
    private VoiceStep? _cutVoice;

    /// <summary>Whether a cutscene still covers the screen.</summary>
    public bool IsShowingStill => _cinematic.IsShowing;

    /// <summary>Whether the black after the last still is still showing.</summary>
    public bool IsLeadingOut => _cinematic.IsLeadingOut;

    /// <summary>How black the cutscene draws over the map, 0..1.</summary>
    public float Blackness => _cinematic.Blackness;

    /// <summary>The cinematic's picture on screen, as a test reads it; null while none shows.</summary>
    internal string? ShownCinematicImage => _cinematic.ShownStill?.ImageFile;

    /// <summary>The skip hint the run last asked for, as a test reads it.</summary>
    internal string SkipHint => view.SkipHint;

    /// <summary>Counts down the black after the last still.</summary>
    /// <returns>Whether the black still shows: the presenter waits on it.</returns>
    public bool AdvanceLeadOut(TimeSpan delta)
    {
        _cinematic.AdvanceLeadOut(delta);
        return IsLeadingOut;
    }

    /// <summary>Shows the current cinematic step, its pictures loaded first (ADR 0046).</summary>
    /// <returns>Whether the cinematic still runs: false once a held skip ended it.</returns>
    public bool Show(TimeSpan delta, InputFrame input)
    {
        _cinematic.Show(exploration.CurrentCinematic!, delta, input);
        return _cinematic.IsRunning;
    }

    /// <summary>Counts the cinematic's pictures as loaded, for a headless test that never draws.</summary>
    internal void MarkStillsLoaded() => _cinematic.MarkStillsLoaded();

    /// <summary>
    /// Plays the voice the cutscene calls for, or stops it: the cinematic's still's voice while one speaks; else the
    /// voice-over the event started, unless a cinematic of the event cut it (decision 622). After the step, so a skip
    /// or the event's end this update silences it at once.
    /// </summary>
    public void FollowStartedVoice()
    {
        var started = exploration.StartedVoice;
        if (started is null)
        {
            // The event ended or was skipped: when it runs again, its voice plays again.
            _cutVoice = null;
        }
        else if (_cinematic.CutsEarlierVoice)
        {
            _cutVoice = started;
        }

        if (_cinematic.Voice is { } cinematicVoice)
        {
            voice.Play(cinematicVoice);
        }
        else if (started is not null && !ReferenceEquals(started, _cutVoice))
        {
            voice.Play(started.File);
        }
        else
        {
            voice.Stop();
        }
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures) => _cinematic.Draw(batch, textures);
}
