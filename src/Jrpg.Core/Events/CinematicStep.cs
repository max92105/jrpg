namespace Jrpg.Core.Events;

/// <summary>
/// Plays a cinematic by id, then waits until the Game has shown it, as a warp waits for its fade (ADR 0046). A skip
/// runs past it and still applies the steps after it (ADR 0016).
/// </summary>
/// <param name="CinematicId">A cinematic of <c>cinematics.json</c>.</param>
public sealed record CinematicStep(string CinematicId) : EventStep
{
    public override bool IsPresentation => true;
}
