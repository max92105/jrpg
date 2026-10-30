namespace Jrpg.Core.Events;

/// <summary>
/// Rests the whole party (ADR 0024), then waits while the player reads it. With a <see cref="Price"/> it is an inn's
/// stay (decision 382): nothing rests until the Game pays through <c>InnStay.TryPay</c>; without one the rest is free
/// and given at once.
/// </summary>
public sealed record RestStep : EventStep
{
    /// <summary>The gold the inn charges, 1 to the purse's most; null for a free rest.</summary>
    public int? Price { get; init; }
}
