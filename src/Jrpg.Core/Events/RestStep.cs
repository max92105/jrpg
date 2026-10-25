namespace Jrpg.Core.Events;

/// <summary>
/// The whole party rests, reserves included (decision 230), then waits while the player reads that they did: an inn, or
/// anywhere a story lets them.
/// </summary>
public sealed record RestStep : EventStep;
