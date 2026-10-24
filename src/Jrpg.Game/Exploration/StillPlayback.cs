using Jrpg.Core.Events;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Where a shown cutscene still is in its time (ADR 0016): it fades in, holds, and fades out over its seconds, so stills
/// "appear and disappear" (decision 159).
/// </summary>
internal sealed class StillPlayback(StillStep still)
{
    /// <summary>How long a still takes to fade in, and again to fade out (placeholder); at most half its seconds.</summary>
    public static readonly TimeSpan ShotFade = TimeSpan.FromSeconds(0.8);

    private TimeSpan _elapsed;

    public StillStep Step => still;

    /// <summary>0 to 1: rising over the fade in, 1 while it holds, falling over the fade out.</summary>
    public float Opacity
    {
        get
        {
            var duration = still.Duration;
            var fade = TimeSpan.FromTicks(Math.Min(ShotFade.Ticks, duration.Ticks / 2));
            if (fade == TimeSpan.Zero)
            {
                return IsOver ? 0 : 1;
            }

            var fadingIn = _elapsed / fade;
            var fadingOut = (duration - _elapsed) / fade;
            return (float)Math.Clamp(Math.Min(fadingIn, fadingOut), 0, 1);
        }
    }

    /// <summary>True once its seconds have passed: the moment it ends belongs to whatever comes next.</summary>
    public bool IsOver => _elapsed >= still.Duration;

    public void Update(TimeSpan delta) => _elapsed += delta;
}
