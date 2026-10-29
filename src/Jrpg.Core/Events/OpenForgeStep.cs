namespace Jrpg.Core.Events;

/// <summary>
/// Opens the Forge, a forge master's crafting screen (decision 353), and waits until the player closes it.
/// </summary>
public sealed record OpenForgeStep : EventStep;
