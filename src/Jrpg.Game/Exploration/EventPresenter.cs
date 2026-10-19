using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Shows the running map event's current step and continues it once shown (ADR 0006): a warp fades to black, moves the
/// player while the screen is black, and fades back in (legacy <c>GameManager.WarpAsync</c>).
/// </summary>
internal sealed class EventPresenter(ExplorationState exploration)
{
    // Legacy SceneManager.TransitionAsync for warps.
    private readonly ScreenFade _warpFade = new(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.08), TimeSpan.FromSeconds(0.25));

    /// <summary>True while an event runs or its fade plays: the player can't walk, interact or open the menu.</summary>
    public bool IsBusy => exploration.IsEventRunning || _warpFade.Phase != FadePhase.Idle;

    /// <summary>How black to draw over the map, 0..1.</summary>
    public float Blackness => _warpFade.Blackness;

    /// <exception cref="InvalidOperationException">The current step is of a type the game can't show yet.</exception>
    public void Update(TimeSpan delta)
    {
        _warpFade.Update(delta);
        switch (exploration.CurrentEventStep)
        {
            case null:
                break;
            case WarpStep:
                ShowWarp();
                break;
            case var step:
                throw new InvalidOperationException($"The game can't show a {step.GetType().Name} yet.");
        }
    }

    private void ShowWarp()
    {
        if (_warpFade.Phase == FadePhase.Idle)
        {
            _warpFade.Start();
        }
        else if (_warpFade.Phase == FadePhase.Holding)
        {
            exploration.ContinueEvent();
        }
    }
}
