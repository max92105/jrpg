using Jrpg.Core.Exploration;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The interaction prompt's opacity as its target appears, changes and goes. A fade instead of a pop, so standing at the
/// edge of an object's reach doesn't make the prompt blink; a new target waits until the old one has faded out.
/// </summary>
internal sealed class PromptFade
{
    /// <summary>From transparent to opaque (placeholder).</summary>
    public static readonly TimeSpan FadeInTime = TimeSpan.FromSeconds(0.15);

    /// <summary>From opaque to transparent (placeholder).</summary>
    public static readonly TimeSpan FadeOutTime = TimeSpan.FromSeconds(0.1);

    private InteractionTarget? _wanted;

    /// <summary>0 for transparent to 1 for opaque.</summary>
    public float Opacity { get; private set; }

    /// <summary>The target being drawn; it stays while it fades out, after another one is wanted.</summary>
    public InteractionTarget? Shown { get; private set; }

    public void Show(InteractionTarget target) => _wanted = target;

    public void Hide() => _wanted = null;

    public void Update(TimeSpan delta)
    {
        if (ReferenceEquals(Shown, _wanted))
        {
            FadeIn(delta);
            return;
        }

        if (Opacity > 0)
        {
            Opacity = Math.Max(0, Opacity - (float)(delta / FadeOutTime));
            return;
        }

        Shown = _wanted;
        FadeIn(delta);
    }

    private void FadeIn(TimeSpan delta)
    {
        if (Shown is not null)
        {
            Opacity = Math.Min(1, Opacity + (float)(delta / FadeInTime));
        }
    }
}
