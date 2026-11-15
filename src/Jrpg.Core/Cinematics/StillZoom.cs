namespace Jrpg.Core.Cinematics;

/// <summary>
/// A still's slow zoom (decision 617): its scale at the start and at the end of its time, in percent of the fitted
/// size, each from <see cref="MinPercent"/> to <see cref="MaxPercent"/>; the scale moves linearly about the center
/// between the two.
/// </summary>
public sealed record StillZoom
{
    /// <summary>The fitted size: a still never shrinks below the screen it fills.</summary>
    public const int MinPercent = 100;

    /// <summary>A small zoom, as the developer asked: past half again, the picture's pixels show.</summary>
    public const int MaxPercent = 150;

    public required int StartPercent { get; init; }

    public required int EndPercent { get; init; }

    /// <summary>
    /// The scale at a share of the still's time, as a factor (108 % is 1.08): linear from start to end.
    /// </summary>
    /// <param name="share">0 at the still's start to 1 at its end.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="share"/> is outside 0 to 1.</exception>
    public double ScaleAt(double share)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(share, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(share, 1);
        return (StartPercent + ((EndPercent - StartPercent) * share)) / 100;
    }
}
