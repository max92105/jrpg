namespace Jrpg.Core.Events;

/// <summary>Adds copies of an item to the inventory, then waits while the player reads what was received.</summary>
public sealed record GiveItemStep(string ItemId, int Quantity) : EventStep;
