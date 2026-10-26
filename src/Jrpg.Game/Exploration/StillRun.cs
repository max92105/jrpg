using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// One run of a cutscene's stills (ADR 0016): black before the first, each still for its seconds, black after the last,
/// so the map never flashes on either side; Confirm or Cancel skips the whole run.
/// </summary>
/// <param name="exploration">The event the stills belong to, continued after each and skipped past them.</param>
/// <param name="view">Draws the still on screen.</param>
internal sealed class StillRun(ExplorationState exploration, StillView view)
{
    // Black before a run of stills and after it (placeholder).
    internal static readonly TimeSpan LeadIn = TimeSpan.FromSeconds(0.5);
    internal static readonly TimeSpan LeadOut = TimeSpan.FromSeconds(0.5);

    // The still on screen; null when no still shows.
    private StillPlayback? _still;

    // The black before the first still of a run (null when not waiting on one), and the black after the last.
    private TimeSpan? _leadInLeft;
    private TimeSpan _leadOutLeft;

    /// <summary>Whether a still covers the screen.</summary>
    public bool IsShowing => _still is not null;

    /// <summary>Whether the black after the last still is still showing.</summary>
    public bool IsLeadingOut => _leadOutLeft > TimeSpan.Zero;

    /// <summary>Whether the run is between its lead-in and the end of its lead-out; false once skipped.</summary>
    public bool IsRunning => _still is not null || _leadInLeft is not null || IsLeadingOut;

    /// <summary>1 during the lead-in and the lead-out, 0 otherwise: the stills cover the screen themselves.</summary>
    public float Blackness => _leadInLeft is not null || IsLeadingOut ? 1 : 0;

    /// <summary>Counts down the black after the last still, while <see cref="IsLeadingOut"/>.</summary>
    public void AdvanceLeadOut(TimeSpan delta) => _leadOutLeft -= delta;

    /// <summary>
    /// Plays <paramref name="still"/>, the event's current step. Confirm or Cancel runs the event past its stills at
    /// any time, the lead-in included, and cuts straight to what follows; the step's first update only starts the
    /// lead-in, so the press that started the event can't skip it.
    /// </summary>
    public void Show(StillStep still, TimeSpan delta, InputFrame input)
    {
        view.Follow(input.Device);
        if (_still is null)
        {
            RunLeadIn(still, delta, input);
            return;
        }

        if (IsSkipPressed(input))
        {
            Skip();
            return;
        }

        _still.Update(delta);
        if (!_still.IsOver)
        {
            return;
        }

        _still = null;
        exploration.ContinueEvent();

        // The next still starts now over the same black, so the map never shows between two stills; after the last, the
        // screen stays black a moment before whatever follows.
        if (exploration.CurrentEventStep is StillStep next)
        {
            Begin(next);
        }
        else
        {
            _leadOutLeft = LeadOut;
        }
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_still is not null)
        {
            view.Draw(batch, textures, _still);
        }
    }

    private void RunLeadIn(StillStep still, TimeSpan delta, InputFrame input)
    {
        if (_leadInLeft is not { } left)
        {
            _leadInLeft = LeadIn;
            return;
        }

        if (IsSkipPressed(input))
        {
            Skip();
            return;
        }

        left -= delta;
        _leadInLeft = left > TimeSpan.Zero ? left : null;
        if (_leadInLeft is null)
        {
            Begin(still);
        }
    }

    private static bool IsSkipPressed(InputFrame input) => input.IsPressed(GameButton.Confirm) || input.IsPressed(GameButton.Cancel);

    private void Skip()
    {
        _still = null;
        _leadInLeft = null;
        exploration.SkipEventPresentation();
    }

    private void Begin(StillStep still)
    {
        _still = new StillPlayback(still);
        view.Begin(still);
    }
}
