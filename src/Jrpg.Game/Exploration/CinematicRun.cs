using Jrpg.Core.Cinematics;
using Jrpg.Core.Exploration;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// One cinematic step on screen (ADR 0046): black while its pictures load, one per frame (decision 628), black a moment
/// more, then each still for its seconds by Core's timeline and fade (decision 629), then black after the last before
/// what follows, so the map never flashes on either side. It also says which still's voice speaks and whether it cuts
/// an earlier voice step's (decisions 613, 622), and skips on a held Confirm (decision 623).
/// </summary>
/// <param name="exploration">The event the cinematic belongs to, continued once its last still ends.</param>
/// <param name="view">Draws the still on screen.</param>
internal sealed class CinematicRun(ExplorationState exploration, StillView view)
{
    // Black before the first still and after the last (placeholder), so the map never flashes between them.
    internal static readonly TimeSpan LeadIn = TimeSpan.FromSeconds(0.5);
    internal static readonly TimeSpan LeadOut = TimeSpan.FromSeconds(0.5);

    // The cinematic playing, from its first update until its last still ends; null otherwise.
    private CinematicDefinition? _cinematic;
    private StillPreload? _preload;
    private TimeSpan? _leadInLeft;
    private TimeSpan _elapsed;
    private StillMoment? _moment;
    private TimeSpan _leadOutLeft;
    private SkipHold _skip = new();

    /// <summary>Whether a still covers the screen.</summary>
    public bool IsShowing => _moment is not null;

    /// <summary>Whether the black after the last still is still showing.</summary>
    public bool IsLeadingOut => _leadOutLeft > TimeSpan.Zero;

    /// <summary>
    /// Whether the cinematic plays or its black after the last still shows; false once a hold skipped it.
    /// </summary>
    public bool IsRunning => _cinematic is not null || IsLeadingOut;

    /// <summary>
    /// 1 while the pictures load, in the black before the first still and after the last; 0 otherwise.
    /// </summary>
    public float Blackness => (_cinematic is not null && _moment is null) || IsLeadingOut ? 1 : 0;

    /// <summary>
    /// The voice line speaking over the still on screen: its own, or the latest earlier still's, which plays on
    /// (decision 613); null while loading, in the black before the first still and after the last.
    /// </summary>
    public string? Voice =>
        _moment is { } moment && _cinematic!.VoicedStillAt(moment.StillIndex) is { } voiced
            ? _cinematic.Stills[voiced].VoiceFile
            : null;

    /// <summary>
    /// Whether this cinematic cuts a voice an earlier step started (decision 622): from its first update when it has a
    /// track, else from its first voiced still; false once it ends.
    /// </summary>
    public bool CutsEarlierVoice => _cinematic is { } cinematic && (cinematic.MusicFile is not null || Voice is not null);

    /// <summary>The still on screen; null while none shows.</summary>
    internal CinematicStill? ShownStill => _moment is { } moment ? _cinematic!.Stills[moment.StillIndex] : null;

    /// <summary>Counts down the black after the last still, while <see cref="IsLeadingOut"/>.</summary>
    public void AdvanceLeadOut(TimeSpan delta) => _leadOutLeft -= delta;

    /// <summary>
    /// Plays <paramref name="cinematic"/>, the event's current step's: its first update starts loading its pictures,
    /// and no time counts until the draw has loaded them all. Holding Confirm for <see cref="SkipHold.HoldTime"/> skips
    /// it at any time after that first update (decision 623); a hold counts only from a press made during the cinematic
    /// (<see cref="SkipHold"/>), so the Confirm that started the event never does.
    /// </summary>
    public void Show(CinematicDefinition cinematic, TimeSpan delta, InputFrame input)
    {
        view.Follow(input.Device);
        if (_cinematic is null)
        {
            Start(cinematic);
            return;
        }

        _skip.Update(delta, input);
        if (_skip.IsDone)
        {
            Skip();
        }
        else if (!_preload!.IsDone)
        {
            return;
        }
        else if (_leadInLeft is { } left)
        {
            AdvanceLeadIn(left - delta);
        }
        else
        {
            _elapsed += delta;
            ShowMoment();
        }
    }

    /// <summary>
    /// While the pictures load, loads the next one and draws nothing (the draw may touch the device, the update never
    /// does, ADR 0008); then the still on screen.
    /// </summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_preload is { Next: { } next } preload)
        {
            textures.Get(ArtFolders.Cinematics, next);
            preload.MarkLoaded();
            return;
        }

        if (_moment is { } moment && ShownStill is { } still)
        {
            var frame = new StillFrame(
                still.ImageFile,
                StillFade.OpacityAt(moment.IntoStill, still.Duration),
                (float)still.ScaleAt(moment.IntoStill));
            view.Draw(batch, textures, frame);
            view.DrawSkipRing(batch, textures, _skip.Progress);
        }
    }

    /// <summary>Counts the pictures as loaded: a headless test never draws, so nothing else loads them.</summary>
    internal void MarkStillsLoaded()
    {
        while (_preload is { IsDone: false } preload)
        {
            preload.MarkLoaded();
        }
    }

    // The first still shows as the black ends; the time left over is dropped, so the still starts at its start.
    private void AdvanceLeadIn(TimeSpan left)
    {
        _leadInLeft = left > TimeSpan.Zero ? left : null;
        if (_leadInLeft is null)
        {
            ShowMoment();
        }
    }

    // A skip cuts straight to what follows: no black after it, which a warp next starts in anyway.
    private void Skip()
    {
        _cinematic = null;
        _preload = null;
        _moment = null;
        _leadInLeft = null;
        exploration.SkipEventPresentation();
    }

    private void Start(CinematicDefinition cinematic)
    {
        _skip = new SkipHold();
        _cinematic = cinematic;
        _preload = new StillPreload(cinematic.Stills);
        _leadInLeft = LeadIn;
        _elapsed = TimeSpan.Zero;
    }

    // A new still measures its subtitle once; past the last, the event goes on behind the black.
    private void ShowMoment()
    {
        if (_cinematic!.MomentAt(_elapsed) is not { } moment)
        {
            End();
            return;
        }

        if (moment.StillIndex != _moment?.StillIndex)
        {
            view.Begin(_cinematic.Stills[moment.StillIndex].Caption);
        }

        _moment = moment;
    }

    private void End()
    {
        _cinematic = null;
        _preload = null;
        _moment = null;
        _leadOutLeft = LeadOut;
        exploration.ContinueEvent();
    }
}
