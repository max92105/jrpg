using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>
/// The flow's screen-change fade and the requests it holds back: the one applied once the screen is black, and one a
/// scene returned while the fade played, released when it ends (plan 2026-10-20-battle, key decision 2).
/// </summary>
internal sealed class FadeGate(ScreenFade fade)
{
    private SceneRequest? _dueAtBlack;
    private SceneRequest? _waiting;

    public bool IsFading => fade.Phase != FadePhase.Idle;

    /// <summary>0 clear to 1 black.</summary>
    public float Blackness => fade.Blackness;

    public void Update(TimeSpan delta) => fade.Update(delta);

    /// <summary>Fades out now; <paramref name="request"/> comes out of <see cref="TakeDueAtBlack"/> once the screen is black.</summary>
    /// <exception cref="InvalidOperationException">The fade is already playing.</exception>
    public void StartBehindFade(SceneRequest request)
    {
        fade.Start();
        _dueAtBlack = request;
    }

    /// <summary>
    /// Keeps <paramref name="request"/> until the fade ends; it then comes out of <see cref="TakeReleased"/> and applies
    /// to the scenes there are then, even if the request applied at black replaced the one that returned it.
    /// </summary>
    /// <exception cref="InvalidOperationException">A request already waits; no scene returns two in one fade.</exception>
    public void Hold(SceneRequest request)
    {
        if (_waiting is not null)
        {
            throw new InvalidOperationException($"A {_waiting.GetType().Name} already waits for the fade; got a {request.GetType().Name}.");
        }

        _waiting = request;
    }

    /// <returns>The request behind the fade once past fading out, once; null otherwise.</returns>
    /// <remarks>Past fading out rather than exactly holding: one long update can carry the fade over the whole hold.</remarks>
    public SceneRequest? TakeDueAtBlack()
    {
        if (fade.Phase == FadePhase.FadingOut)
        {
            return null;
        }

        var request = _dueAtBlack;
        _dueAtBlack = null;
        return request;
    }

    /// <returns>The held request once the fade has ended, once; null otherwise.</returns>
    public SceneRequest? TakeReleased()
    {
        if (IsFading)
        {
            return null;
        }

        var request = _waiting;
        _waiting = null;
        return request;
    }
}
