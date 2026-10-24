namespace Jrpg.Core.Events;

/// <summary>
/// Shows an image over the whole screen for a time, with an optional subtitle, then waits until the Game has shown it,
/// as a warp waits for its fade (ADR 0016).
/// </summary>
/// <param name="ImageFile">A file in the cinematics art folder.</param>
/// <param name="Seconds">How long it shows, above 0; set in content to match the voice-over (decision 173).</param>
/// <param name="Caption">The subtitle under it, or null (decision 172).</param>
public sealed record StillStep(string ImageFile, double Seconds, string? Caption) : EventStep
{
    public TimeSpan Duration => TimeSpan.FromSeconds(Seconds);

    public override bool IsPresentation => true;
}
